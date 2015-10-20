'''
This is the file where you will implement
your logic for interpreting a Prefab
tree.

The methods here work like event handlers:
each method is executed by Prefab when certain
events occur. 

interpret is called when:
Prefab is given a new screenshot to interpret

process_annotations is called when:
(1) An annotation is added/removed
(2) This layer is loaded and it has been 
    edited after it was previously loaded
'''

from Prefab import *
from PrefabSingle import *

MAX_SPACE_DIST = 10
MAX_ROW_DIST = 10




def interpret(interpret_data):
	'''
	Implement this method to interpret a
	tree that results from the execution
	of Prefab.
	
	interpret_data.tree:
		This is a mutable object that represents
		the current interpretation of a screenshot.
		
		Each node has a dictionary of tags:
		tagvalue = tree['tagname']
		tree['is_text'] = True
		
		A bounding rectangle:
		width = tree.width
		height = tree.height
		top = tree.top
		left = tree.left

		A list of children that are contained
		spatially within the node's boundary:
		children = tree.get_children()
		children.add( node )
		
	interpret_data.runtime_storage:
		This is a dict where you can access your annotations. 
		from path_utils import path. Keys to the dict are
		path descriptors and values are dicts that contain
		metadata about the corresponding tree node.
		
		path = get_path( node )
		if path in runtime_storage:
			node['is_text'] = runtime_storage[path]['is_text']
	'''
	find_leaves_tag_as_text(interpret_data.tree)
	apply_text_corrections(interpret_data, interpret_data.tree)
	from tag_widget_type import recursively_tag_group_next, recursively_apply_groups
	recursively_tag_group_next(interpret_data.tree)
	recursively_apply_groups(interpret_data.tree)

def find_leaves_tag_as_text(currnode):
	'''
	Recursively walk through tree and tag leaves as text.
	'''
	if len( currnode.get_children() ) == 0:
		currnode['is_text'] = True
	
	for child in currnode.get_children():
		find_leaves_tag_as_text(child)


def apply_text_corrections(interpret_data, currnode):
	'''
	Walk through tree and use path descriptors to
	overwrite erroneous tags.
	'''
	from path_utils import get_path
	#importing the method to compute path descriptors
	
	path = get_path(currnode, interpret_data.tree)
	#computing the path descriptor for the current node
	
	
	if path in interpret_data.runtime_storage:
		#if there is an annotation for this node and it has 'is_text' metadata
		#then tag the node with that data
		if 'is_text' in interpret_data.runtime_storage[path]:
			correction = interpret_data.runtime_storage[path]['is_text']
			currnode['is_text'] = correction
	
	#recurse on the children of currnode
	for child in currnode.get_children():
		apply_text_corrections(interpret_data, child)