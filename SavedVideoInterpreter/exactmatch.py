import path_utils

def get_annotation_libraries( args):
	if args.Parameters.ContainsKey('library'):
		return [args.Parameters['library'] ]
	return  [ ]
	
def interpret(args ):
	root = args.root
	saveddata = args.saved_data


	interpret_helper( root, root, args)
	
def interpret_helper(node, root, args):

	if args.parameters.ContainsKey("use_path") and args.Parameters["use_path"] == "true":
		path = path_utils.get_path(node, root)
	else:
		path = path_utils.get_prototype_id_path(node)
	
	if path in args.saved_data:
		keys = args.saved_data[path]['keys']
		
		for key in keys:
			key = key.ToString()
			val = args.saved_data[path][key].ToString()
			args.set_attribute( node, key, val)
			keyid = args.saved_data[path][key + '-id' ].ToString()
			args.set_attribute(node, key + '-id', keyid)
		
			srclib = args.saved_data[path][key + '-lib'].ToString()
			args.set_attribute(node, key + '-lib', srclib)
		
		
	children = node.GetChildren()
	for child in children:
		interpret_helper( child, root, args)
		
def process_annotations( args ):
	args.saved_data.Clear()
	
	for node in args.annotated_nodes:
		if node.Node:
			if args.parameters.ContainsKey("use_path") and args.Parameters["use_path"] == "true":
				path = path_utils.get_path(node.Node, node.Root)
			else:
				path = path_utils.get_prototype_id_path(node.Node)
			
			data = args.saved_data[path]
			if not data:
				data = args.saved_data.CreateItem()
			
			key = node.Data['key'].ToString()
			val = node.Data['value']	
			
			data['keys'].Add( key )	
			
			data[key] = val
			
			data[ key + '-id' ] = node.Data['id']
			data[ key + '-lib' ] = node.Data['lib']	
			args.saved_data[path] = data
		
		