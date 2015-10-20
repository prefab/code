'''
This is a template file you can use 
to help you write a layer. 

The methods here work like event handlers:
each method is executed by Prefab when certain
events occur. 

interpret is called when:
Prefab is given a new screenshot to interpret

generalize_annotations is called when:
(1) An annotation is added/removed
(2) This layer is loaded and it has been 
    edited after it was previously loaded
(3) Any layers preceding this layer were 
    loaded and have changed
'''





def generalize_annotations(annotation_data):
	'''
	Implement this method to generalize your annotations.
	For example, you might generate path descriptors for
	each annotated node so that you can find these same 
	nodes when interpreting a new screenshot.
	Or you might create a classifier using the annotations
	as training data.
	
	annotation_data.annotated_nodes:
		This argument is a list of annotations.
		An annotation consists of a tree node and 
		the associated data. The associated data is 
		a dict containing key/value pairs. It also
		It also contains the root node to the entire 
		tree containing this annotation. See the
		interpret method for the specifications of
		the tree node object.
		
		annotation = annotated_nodes[0]
		is_text = annotation.data['is_text']
		node = annotation.node
		root = annotation.root
		
	annotation_data.runtime_storage:
		This is a dict where you can store any information
		generalized from your annotations, such as path
		descriptors or learned classifiers. This data will be
		passed into the interpret method, and it will also be
		serialized to disk so that the layer does not need to
		recompute everything if Prefab exits and restarts.

		annotation = annotated_nodes[0]
		path = get_path( annotation.node )
		runtime_storage[path] = annotation.data
	
	annotation_data.parameters:
		This is a dict of developer-provided parameters
		the layer can access to parameterize its behavior.
		The same parameters also available in the interpret method.
		For example, if you are translating the language of inter
		the layer might accept the specific language as a parameter.
		translation_language = parameters['language']
		
	Here's an example that computes and stores path 
	descriptors with the annotation data.
	'''
	runtime_storage = annotation_data.runtime_storage
	runtime_storage.clear()
	annotated_nodes = annotation_data.annotated_nodes
	#importing the script for computing path descriptors
	from path_utils import get_path 
	for anode in annotated_nodes:
		path = get_path( anode.node, anode.root)
		runtime_storage[path] = anode.data

		
		
		
def interpret(interpret_data):
	'''
	Implement this method to interpret a
	tree that results from the execution of layers 
	preceding this layer.
	
	interpret_data.tree:
		This is a read-only object that represents
		the current interpretation of a screenshot.
		
		Each node has a dictionary of tags:
		tagvalue = tree['tagname']
		
		A bounding rectangle:
		width = tree.width
		height = tree.height
		top = tree.top
		left = tree.left

		A list of children that are contained
		spatially within the node's boundary:
		children = tree.get_children()
		
	interpret_data.runtime_storage:
		This is a dict where you can store any information
		generalized from your annotations. See the 
		generalize_annotations method.

	interpret_data.tree_transformer:
		This is an object that will queue transformations
		that will be executed after this layer completes.
		enqueue_set_tag(node, tagname, tagvalue)
		enqueue_set_ancestor(node, ancestor_node)
		enqueue_delete(node)
		newnode = create_node(x, y, width, height, tags = {} )
		
	interpret_data.parameters:
		This is a dict of developer-provided parameters
		the layer can access to parameterize its behavior.
		
		
	Here's an example that retrieves path descriptors
	from the runtime_storage and tags matching nodes
	with the associated data.
	
	
	'''
	recursively_tag_nodes(interpret_data, interpret_data.tree)

	

def recursively_tag_nodes(interpret_data, currnode):
	'''
	This method recursively tags nodes
	that match path descriptors stored in 
	the runtime_storage
	'''	
	transformer = interpret_data.tree_transformer
	from path_utils import get_path 
	#import the method to retrieve path descriptors
	
	path = get_path(currnode, interpret_data.tree)
	if path in interpret_data.runtime_storage:
		#check if there is a corresponding path descriptor stored
		data = interpret_data.runtime_storage[path]
		for key in data:
			transformer.enqueue_set_tag(currnode, key, data[key])
	
	for child in currnode.get_children():
		recursively_tag_nodes(interpret_data, child)

	
	