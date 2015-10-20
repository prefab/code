
from Prefab import *

MAX_SPACE_DIST = 12;
MAX_ROW_DIST = 12;



def interpret(args):
	interpret_helper(args, args.root)
	
	
def interpret_helper(args, currnode):

	text_children = [x for x in currnode.GetChildren() if x['is_text'] and bool(x['is_text'])]
	
	reading_order = sort_nodes_reading_order(text_children)
		
	for i in range(0, len(reading_order) - 1):
		curr = reading_order[i]
		next = reading_order[i+1]
		
		args.tag(curr, 'group_next', decide_group(curr, next))
		
	for child in currnode.GetChildren():
		interpret_helper(args, child)


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
	
	text_sorted_by_height = sorted(text_sorted_by_height, key=lambda t: t.Height)
	
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
		rows[i] = sorted(rows[i], key=lambda t: t.Left)
		
	rows = sorted(rows, key=lambda r : r[0].Top)
	
	return rows
				

def decide_group(curr, next):
	leftdist = 0
	topdist = 0
	if BoundingBox.IsAlignedVertically(curr, next):
		leftdist = 0
	elif curr.Left < next.Left:
		leftdist = next.Left - (curr.Left + curr.Width)
	else:
		leftdist = curr.Left - (next.Left + next.Width)
		
	if BoundingBox.IsAlignedHorizontally(curr, next):
		topdist = 0
	elif curr.Top < next.Top:
		topdist = next.Top - (curr.Top + curr.Height)
	else:
		topdist = curr.Top - (next.Top + next.Height)
	
	closexdist = leftdist < MAX_SPACE_DIST
	closeydist = topdist < MAX_ROW_DIST
	
	return closexdist and closeydist