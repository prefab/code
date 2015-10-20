import tag_group_next
from Prefab import *


def interpret(interpret_data):
	recursively_group(interpret_data, interpret_data.tree)
	

def recursively_group(interpret_data, currnode):
	'''
	This method visits each node and groups any of its children
	based on the 'group_next' tags.
	'''
	children = [x for x in currnode.get_children() if x['consider_group']]
	sorted = tag_group_next.sort_nodes_reading_order( children )
	
	createnew = True
	togroup = []
	
	for i in range(1, len(sorted)):
		currchild = sorted[i]
		prevchild = sorted[i-1]
		
		if prevchild['group_next']:
			if createnew:
				createnew = False
				togroup.append(prevchild)
				
			togroup.append(currchild)
		else:
			if len(togroup) > 0:
				group_nodes(togroup, interpret_data, currnode)	
				togroup = []
			createnew = True
		
	if len( togroup ) > 0:
		group_nodes(togroup, interpret_data, currnode)
		togroup = []
		
	for child in currnode.get_children():
		recursively_group(interpret_data, child)
		
def group_nodes(togroup, interpret_data, currnode):
	bounding = BoundingBox.Union(togroup)
	tags = {}
	tags['type'] = 'grouped_text'
	tags['is_text'] = True
	grouped = interpret_data.create_node(bounding, tags)
	
	for node in togroup:
		interpret_data.enqueue_set_ancestor(node, grouped)
	
	interpret_data.enqueue_set_ancestor(grouped, currnode)
