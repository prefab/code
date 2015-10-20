

def interpret(interpret_data):
	recursively_tag_leaves(interpret_data, interpret_data.tree)
	
	
def recursively_tag_leaves(interpret_data, currnode):
	if currnode.Height > 3 and currnode['type'] == 'content':
		args.tag(currnode, 'is_text', True)
		
	for child in currnode.get_children():
		recursively_tag_leaves(args, child)