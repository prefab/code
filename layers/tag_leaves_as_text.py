

def interpret(interpret_data):
	find_leaves_and_tag_as_text(interpret_data, interpret_data.tree)
	
	
def find_leaves_and_tag_as_text(interpret_data, currnode):
	if len( currnode.get_children() ) == 0:
		interpret_data.enqueue_set_tag(currnode, 'is_text', True)
		
	for child in currnode.get_children():
		find_leaves_and_tag_as_text(interpret_data, child)