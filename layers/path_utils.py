from Prefab import *
from PrefabUtils import *
def get_path(node, root):
	path = PathDescriptor.GetPath(node, root)
	#resultarray = []
	#get_path_helper(node, root, resultarray)
	#path =  ''.join(resultarray)

	return path
	
	
def get_path_helper(node, root, path):
	if not node:
		return
	
	parent = get_parent(node, root)
	get_local_path(node, parent, path)
	
	if parent:
		path.append('/')
		get_path_helper(parent, root, path)
		
def get_parent(node, root):
	if root == None or node == None:
		return None

	if root == node:
		return None;

	if node in root.get_children():
		return root


	for child in root.get_children():
		found = get_parent(node, child);
		if (found != None):
			return found
			

	return None

def get_local_path(node, parent, path):
	if node['type']:
		type = node['type']
		if type == 'ptype':
			path.append(str(node['ptype_id']))
		elif type == 'frame':
			path.append('frame')
		elif type == 'content':
			path.append("content[@pixel_hash=" + str(node["pixel_hash"]) + "]")
		else:
			path.append(type)
			
		nodeindex = get_node_index(node, parent)
		path.append("[" + str(nodeindex) + "]")
		
def get_node_index(node, parent):
	type = None
	index = 0
	pixelHash = None
	
	if (node["is_content"] and node["is_content"] == "true"):
		pixelHash = node["pixel_hash"]
	
	if parent:
		children = parent.get_children()
		for sibling in children:
			if sibling == node:
				return index
			elif sibling['ptype_id'] == node['ptype_id']:
				if pixelHash and sibling['is_content'] and sibling['is_content'] == 'true' and sibling['pixel_hash'] == pixelHash:
					index = index + 1
				elif not pixelHash:
					index = index + 1
	return index
	
	