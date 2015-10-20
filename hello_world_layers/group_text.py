
import group_next
from Prefab import *

def interpret(args):
	interpret_helper(args, args.root)
	

def interpret_helper(args, currnode):
	children = children = [x for x in currnode.GetChildren() if x['is_text'] and bool(x['is_text'])]
	
	sorted = group_next.sort_nodes_reading_order( children )
	
	createnew = True
	togroup = []
	
	for i in range(1, len(sorted)):
		currchild = sorted[i]
		prevchild = sorted[i-1]
		
		if prevchild['group_next'] and bool(prevchild['group_next']):
			if createnew:
				createnew = False
				togroup.append(prevchild)
				
			togroup.append(currchild)
		else:
			if len(togroup) > 0:
				group_nodes(togroup, args, currnode)
				togroup = []
			createnew = True
		
	if len( togroup ) > 0:
		group_nodes(togroup, args, currnode)
		togroup = []
	for child in currnode.GetChildren():
		interpret_helper(args, child)
		
def group_nodes(togroup, args, currnode):
	bounding = BoundingBox.Union(togroup)
	tags = {}
	tags['type'] = 'grouped_text'
	tags['is_text'] = True
	grouped = args.create_node(bounding, tags)
	
	for node in togroup:
		args.set_ancestor(node, grouped)
	
	args.set_ancestor(grouped, currnode)
