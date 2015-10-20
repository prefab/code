# This basic algorithm inserts nodes into the tree based on the previous tree. Any node in the old  
# tree that is missing from the new tree is added to the new tree. It only inserts nodes that have disappeared
# within a short period of time

# here's the algorithm
#remove all expired timestamps from a table of timestamps (using threshhold)
#for each node in the current hierarhcy, update (or initialize) its timestamp in the table
#for each node in previous hierarchy, if it's missing from the current one (by comparing bounding region)
#then check if it has a timestamp - if so, add the node to the current hierarchy

from Prefab.Interpretation import Tree
import time

timestamps = {}
prev_root = None

timethresh = 1500

def interpret(args):
	global prev_root
	root = args.root

	if root["is_migrating" ]:
		return True

	clear_expired()
	
	update_timestamps(root)
		
	if prev_root:
		add_previous( prev_root, root, args)
	
	
def after_interpret( tree ):
	global prev_root
	prev_root = tree

def add_previous( node, currRoot , args):
	global timestamps
	global prev_root
	key = get_key(node)
	
	#see if the new tree is missing this node
	if not get_match( key, currRoot ):

		#it's missing the node - let's see if the node has expired
		if key in timestamps:
			
			#it didn't expire - let's find the corresponding parent in the new tree
			oldparent = Tree.GetParent( node, prev_root )
			
			if oldparent:
				newparent = get_match( get_key(oldparent), currRoot )
				
				if newparent:
					#we got the corresponding parent, let's add the node
					args.set_ancestor( node, newparent)
	else:
		#the new tree has this node, so let's recursively check its children
		for child in node.GetChildren():
			add_previous( child, currRoot, args)

def get_match( key, node):
	otherkey = get_key(node)
	if key == otherkey:
		return node
	
	for child in node.GetChildren():
		if get_match( key, child):
			return child
	
	return None

def clear_expired():
	global timestamps
	todelete = []
	for stamp in timestamps:
		then = timestamps[stamp]
		now =  time.clock()
		delta = now - then
		delta = delta * 1000
		if delta >= timethresh:
			todelete.append(stamp)
			
	for stamp in todelete:
		del timestamps[stamp]
			

def get_key( node ):
	key = 17
	key = 31 * key + node.TopOffset
	key = 31 * key + node.LeftOffset
	key = 31 * key + node.Width
	key = 31 * key + node.Height
	
	return key

def update_timestamps( node ):
	global timestamps
	key = get_key(node)
	now = time.clock()
	timestamps[key] = now
	
	for child in node.GetChildren():
		update_timestamps( child )
		
		