from Prefab import *

MAX_SPACE_DIST = 10
MAX_ROW_DIST = 4


def interpret(interpret_data):
	recurse_each_node_and_group_children(interpret_data, interpret_data.tree)
	
	
def recurse_each_node_and_group_children(interpret_data, currnode):
	'''
	This method visits each node, sorts that node's children in
	reading order, and then decides if each one should be grouped
	with the next.
	'''

	#get all of the text nodes so that they can be grouped
	children_to_sort = [x for x in currnode.get_children() 
		if x['is_text'] and str(x['is_text']).lower() == 'true'
		and x.height > 3]
	
	for node in children_to_sort:
		interpret_data.enqueue_set_tag(node, 'consider_group', True)
	
	#sort the children in reading order
	reading_order = sort_nodes_reading_order(children_to_sort)
	
	#decide for each child if it should be grouped with the next 
	for i in range(0, len(reading_order) - 1):
		curr = reading_order[i]
		next = reading_order[i+1]
		
		interpret_data.enqueue_set_tag(curr, 'group_next', decide_group(curr, next))
	
	#recurse on the children
	for child in currnode.get_children():
		recurse_each_node_and_group_children(interpret_data, child)
		


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
				break
		if not added:
			row = [ txt ]
			rows.append(row)
			
	for i in range(0, len(rows)):
		rows[i] = sorted(rows[i], key=lambda t: t.left)
		
	rows = sorted(rows, key=lambda r : r[0].top)
	
	return rows
	

