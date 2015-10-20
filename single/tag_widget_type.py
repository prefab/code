'''
This is the file where you will implement
your logic for interpreting a Prefab
tree.

The methods here work like event handlers:
each method is executed by Prefab when certain
events occur. 

interpret is called when:
Prefab is given a new screenshot to interpret

process_annotations is called when:
(1) An annotation is added/removed
(2) This layer is loaded and it has been 
    edited after it was previously loaded
'''

from Prefab import *
from PrefabSingle import *

MAX_SPACE_DIST = 10
MAX_ROW_DIST = 10




def interpret(interpret_data):
	'''
	Implement this method to interpret a
	tree that results from the execution
	of Prefab.
	
	interpret_data.tree:
		This is a mutable object that represents
		the current interpretation of a screenshot.
		
		Each node has a dictionary of tags:
		tagvalue = tree['tagname']
		tree['is_text'] = True
		
		A bounding rectangle:
		width = tree.width
		height = tree.height
		top = tree.top
		left = tree.left

		A list of children that are contained
		spatially within the node's boundary:
		children = tree.get_children()
		children.add( node )
		
	interpret_data.runtime_storage:
		This is a dict where you can access your annotations. 
		from path_utils import path. Keys to the dict are
		path descriptors and values are dicts that contain
		metadata about the corresponding tree node.
		
		path = get_path( node )
		if path in runtime_storage:
			node['is_text'] = runtime_storage[path]['is_text']
	'''
	find_leaves_tag_as_text(interpret_data.tree)
	apply_text_corrections(interpret_data, interpret_data.tree)
	recursively_tag_group_next(interpret_data.tree)
	recursively_apply_groups(interpret_data.tree)
	recursively_tag_widget_type(interpret_data, interpret_data.tree)
	recursively_replace_widget_type(interpret_data, interpret_data.tree)

def find_leaves_tag_as_text(currnode):
	'''
	Recursively walk through tree and tag leaves as text.
	'''
	if len( currnode.get_children() ) == 0:
		currnode['is_text'] = True
	
	for child in currnode.get_children():
		find_leaves_tag_as_text(child)


def apply_text_corrections(interpret_data, currnode):
	'''
	Walk through tree and use path descriptors to
	overwrite erroneous tags.
	'''
	from path_utils import get_path
	#importing the method to compute path descriptors
	
	path = get_path(currnode, interpret_data.tree)
	#computing the path descriptor for the current node
	
	
	if path in interpret_data.runtime_storage:
		#if there is an annotation for this node and it has 'is_text' metadata
		#then tag the node with that data
		if 'is_text' in interpret_data.runtime_storage[path]:
			correction = interpret_data.runtime_storage[path]['is_text']
			currnode['is_text'] = correction
	
	#recurse on the children of currnode
	for child in currnode.get_children():
		apply_text_corrections(interpret_data, child)

def recursively_tag_group_next(currnode):
	'''
	This method visits each node, sorts that node's children in
	reading order, and then decides if each one should be grouped
	with the next.
	'''
	
	#recurse on the children
	for child in currnode.get_children():
		recursively_tag_group_next(child)
	
	#get all of the text nodes so that they can be grouped
	children_to_sort = [x for x in currnode.get_children() 
		if x['is_text'] and str(x['is_text']).lower() == 'true'
		and x.height > 3]	
		
	#sort the children in reading order
	reading_order = sort_nodes_reading_order(children_to_sort)
	
	#decide for each child if it should be grouped with the next 
	for i in range(0, len(reading_order) - 1):
		
		curr = reading_order[i]
		next = reading_order[i+1]
		curr['group_next'] = decide_group(curr,next)
	

def recursively_apply_groups(currnode):
	#group the children that we have tagged to be grouped
	
	#recurse on children
	for child in currnode.get_children():
		recursively_apply_groups(child)

	#get all of the text nodes so that they can be grouped
	children_to_sort = [x for x in currnode.get_children() 
		if x['is_text'] and str(x['is_text']).lower() == 'true'
		and x.height > 3]	
	#sort the children in reading order
	reading_order = sort_nodes_reading_order(children_to_sort)
	
	
	createnew = True
	togroup = []
	for i in range(1, len(reading_order)):
		currchild = reading_order[i]
		prevchild = reading_order[i-1]
		
		
		if prevchild['group_next']: #true if we create a group
			if createnew: #if we need to create a new group
				createnew = False
				togroup.append(prevchild)
				
			togroup.append(currchild)
		else: #we don't add this node, so let's perform the grouping on what we've collected
			if len(togroup) > 0:
				group_nodes(togroup, currnode)
				togroup = []
			createnew = True
		
	if len( togroup ) > 0: #check if we have one set of nodes left to group
		group_nodes(togroup, currnode)
		togroup = []
		

	
def recursively_tag_widget_type(interpret_data, currnode):
	'''
	This method visits each node in the hierarchy and
	tags it with its widget type. It heuristically guesses
	that elements larger than 100x100 are containers,
	and it uses stored annotations to override that label.
	'''
	from path_utils import get_path
	path = get_path(currnode, interpret_data.tree)
	#import method to compute path descriptor
	
	
	runtime_storage = interpret_data.runtime_storage
	
	if path in runtime_storage:
		if 'widget_type' in runtime_storage[path]:
			currnode['widget_type'] = runtime_storage[path]['widget_type']
		
	elif currnode.width > 100 and currnode.height > 100:
		
		currnode['widget_type'] = 'container'
		
	for child in currnode.get_children():
		recursively_tag_widget_type(interpret_data, child)
		
		
def recursively_replace_widget_type(interpret_data, currnode):
	'''
	This method tags each node with a replacement widget type
	that is suited for a touchscreen rather than a mouse and
	keyboard. The application will then perform the replacement
	using the output of this script.
	'''
	runtime_storage = interpret_data.runtime_storage
	if currnode['widget_type'] and currnode['widget_type'] in runtime_storage:
		currnode['replacement_type'] = runtime_storage['widget_type']['replacement_type']
	
	for child in currnode.get_children():
		recursively_replace_widget_type(interpret_data, child)

		
def decide_group(curr, next):
	'''
	This method uses thresholds to heuristically 
	decide if two nodes adjacent in reading order
	should be grouped together or not.
	'''
	leftdist = 0
	topdist = 0
	if BoundingBox.IsAlignedVertically(curr, next):
		leftdist = 0
	elif curr.left < next.left:
		leftdist = next.left - (curr.left + curr.width)
	else:
		leftdist = curr.left - (next.left + next.width)
		
	if BoundingBox.IsAlignedHorizontally(curr, next):
		topdist = 0
	elif curr.top < next.top:
		topdist = next.top - (curr.top + curr.height)
	else:
		topdist = curr.top - (next.top + next.height)
	
	closexdist = leftdist < MAX_SPACE_DIST
	closeydist = topdist < MAX_ROW_DIST
	
	return closexdist and closeydist

def sort_nodes_reading_order(siblings):
	sorted = []
	rows = nodes_by_rows(siblings)
	
	for row in rows:
		for node in row:
			sorted.append(node)
			
	return sorted
	

	
def nodes_by_rows( text ):
	rows = []
	text_sorted_by_height = []
	text_sorted_by_height.extend( text )
	
	text_sorted_by_height = sorted(text_sorted_by_height, key=lambda t: -t.height)
	
	for txt in text_sorted_by_height:
		added = False
		for row in rows:
			if BoundingBox.IsAlignedHorizontally(row[0], txt):
				row.append(txt)
				added = True
		if not added:
			row = [ txt ]
			rows.append(row)
			
	for i in range(0, len(rows)):
		rows[i] = sorted(rows[i], key=lambda t: t.left)
		
	rows = sorted(rows, key=lambda r : r[0].top)
	
	return rows
	
def group_nodes(togroup, currnode):
	bounding = BoundingBox.Union(togroup)
	tags = {}
	tags['type'] = 'grouped_text'
	tags['is_text'] = True
	grouped = CreateTree.from_bounding_box(bounding, tags)
	
	grouped_children = grouped.get_children()
	currnode_children = currnode.get_children()
	
	for node in togroup:
		grouped_children.Add(node)
		currnode_children.Remove(node)
	currnode_children.Add(grouped)
	
	
def process_annotations(annotation_data):

	#Removing the path descriptors containing "grouped_text"
	#import re
	#paths_to_remove = []
	#expression = r'grouped_text\[\d+\]'
	#expression = re.compile(expression)
	#for path in annotation_data:
	#	if expression.match( path ):
	#		paths_to_remove.append (path)
			
	#Removing the old paths and adding the new ones
	#for toremove in paths_to_remove:
	#	metadata = annotation_data[ toremove  ]
	#	del annotation_data[ toremove ]
	pass