def get_annotation_libraries( args):
	if args.Parameters.ContainsKey('library'):
		return [args.Parameters['library']]
		
	return  []
	
	
def interpret(args ):
	root = args.root
	
	saveddata = args.saved_data
	
	interpret_helper( root, root, args  )
	
def interpret_helper(node, root, args):
	
	prototype_id = node.Occurrence.Prototype.Guid.ToString("D")
	
	if prototype_id in args.saved_data:
	
		keys = args.saved_data[prototype_id]['keys']
		
		for key in keys:
			key = key.ToString()
			val = args.saved_data[prototype_id][key].ToString()
			args.set_attribute( node, key, val)
			
			keyid = args.saved_data[prototype_id][key + '-id'].ToString()
			args.set_attribute(node, key + '-id', keyid)
		
			srclib = args.saved_data[prototype_id][key + '-lib'].ToString()
			args.set_attribute(node, key + '-lib', srclib)

	children = node.GetChildren()
	for child in children:
		interpret_helper( child, root, args)
		
		
def process_annotations( args ):
	
	args.saved_data.Clear()
	
	for node in args.annotated_nodes:
		if node.Node:
			prototype_id = node.Node.Occurrence.Prototype.Guid.ToString("D" )
			data = args.saved_data[prototype_id]
			if not data:
				data = args.saved_data.CreateItem()
			key = node.Data['key'].ToString()
			val = node.Data['value']	
			data['keys'].Add( key )	
			data[key] = val
			data[ key + '-id' ] = node.Data['id']
			data[ key + '-lib' ] = node.Data['lib']	
			args.saved_data[prototype_id] = data
		
		