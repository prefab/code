import sys
from Prefab.Utils import *
from Prefab.Prototypes import *
from Prefab.Identification import *
import math

MAX_SPACE_DIST = 5
MAX_ROW_DIST = 4

class GroupedText:
	def __init__(self, first):
		self.occurrences = [ first ]
		self.bounding_box = first
		
	def add(self, txt_occurrence):
		self.occurrences.append(txt_occurrence)
		self.bounding_box = BoundingBox.Union(self.bounding_box, txt_occurrence)
		
	def get_occurrences(self):
		return self.occurrences

def update_node_children(parent, grouped_children, args):
	for grouped in grouped_children:
		bb = grouped.bounding_box
		attrs = { "grouped_content" : "true" }
		groupnode = args.create_node(bb.LeftOffset, bb.TopOffset, bb.Width, bb.Height, attrs)
		args.set_ancestor(groupnode, parent)
		for node in grouped.get_occurrences():
			args.set_ancestor(node, groupnode)

def get_closest(node, groups):
	mindist = sys.float_info.max
	closest = None
	
	for set in groups:
		xdist = 0
		ydist = 0
		dist = 0

		if not BoundingBox.IsAlignedVertically(node, set.bounding_box):
			if node.LeftOffset < set.bounding_box.LeftOffset:
				xdist = set.bounding_box.LeftOffset - (node.LeftOffset + node.Width)
			else:
				xdist = node.LeftOffset - (set.bounding_box.LeftOffset + set.bounding_box.Width)
		
		if not BoundingBox.IsAlignedHorizonally(node, set.bounding_box):
			ydist = node.TopOffset - (set.bounding_box.TopOffset + set.bounding_box.Height);
		
		dist = math.sqrt(xdist * xdist + ydist * ydist);

		if (dist < mindist):
			mindist = dist
			closest = set
			
	return closest

	

def interpret_helper(node, args):
	children = node.GetChildren()
	children = [x for x in children if x['is_content'] == 'true']
	children.sort( key = lambda node : node.LeftOffset )
	
	if len( children ) > 0:
		
		grouped = [ GroupedText(children[0]) ]
		
		for txt in children[1:]:
			
			nearest = get_closest(txt, grouped)
			right = nearest.bounding_box.LeftOffset + nearest.bounding_box.Width
			bottom = nearest.bounding_box.TopOffset + nearest.bounding_box.Height
			if BoundingBox.IsAlignedHorizonally(txt, nearest.bounding_box) and (txt.LeftOffset < right + MAX_SPACE_DIST) and (txt.TopOffset < bottom + MAX_ROW_DIST):
				nearest.add(txt)
			else:
				next = GroupedText(txt)
				grouped.append( next )
				
		update_node_children(node, grouped, args)
	
	for child in node.GetChildren():
		interpret_helper(child, args)
	
def interpret(args):
	node = args.root
	interpret_helper(node, args)
	
	