def interpret(interpret_data):
	recursively_tag_leaves(interpret_data, interpret_data.tree)
	
	
def recursively_tag_leaves(interpret_data, currnode):
	if len( currnode.get_children()) == 0:
		interpret_data.enqueue_set_tag(currnode, 'is_target', True)
		
	for child in currnode.get_children():
		recursively_tag_leaves(interpret_data, child)