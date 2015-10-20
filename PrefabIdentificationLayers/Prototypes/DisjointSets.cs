using System;

namespace PrefabIdentificationLayers.Prototypes
{
	public class DisjointSets
	{

		/// <summary>
		/// The list of nodes representing the elements.
		/// </summary>
		private int[] parent;
		private int[] rank;

		/// <summary>
		/// Create a DisjointSets data structure with a specified number of elements (with element id's from 0 to count-1)
		/// </summary>
		/// <param name="count"></param>
		public DisjointSets(int bufferSize)
		{
			parent = new int [ bufferSize ];
			rank = new int[bufferSize];

			for( int i = 0; i < parent.Length; i++ ){
				parent[ i ] = i;
				rank[i] = 0;
			}
		}

		/// <summary>
		/// Find the set identifier that an element currently belongs to.
		/// Note: some internal data is modified for optimization even though this method is consant.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public int Find( int x )
		{
			if( parent[ x ] == x )
				return x;
			else
			{
				int result = Find( parent[ x ] );
				parent[x] = result;
				return result;
			}
		}



		public bool union( int i, int j )
		{
			// Find the representatives (or the root nodes) for the set that includes i
			int irep = Find(i),
			// And do the same for the set that includes j
			jrep = Find(j),
			// Get the rank of i's tree
			irank = rank[irep],
			// Get the rank of j's tree
			jrank = rank[jrep];

			// Elements are in the same set, no need to unite anything.
			if (irep == jrep)
				return false;

			// If i's rank is less than j's rank
			if (irank < jrank) {

				// Then move i under j
				parent[irep] = jrep;

			} // Else if j's rank is less than i's rank
			else if (jrank < irank) {

				// Then move j under i
				parent[jrep] = irep;

			} // Else if their ranks are the same
			else {

				// Then move i under j (doesn't matter which one goes where)
				parent[irep] = jrep;

				// And increment the the result tree's rank by 1
				rank[jrep]++;
			}

			return true;
		}

	}
}

