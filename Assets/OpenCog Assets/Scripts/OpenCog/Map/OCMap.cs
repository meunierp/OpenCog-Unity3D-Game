
/// Unity3D OpenCog World Embodiment Program
/// Copyright (C) 2013  Novamente			
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the GNU Affero General Public License as
/// published by the Free Software Foundation, either version 3 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
/// GNU Affero General Public License for more details.
///
/// You should have received a copy of the GNU Affero General Public License
/// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#region Usings, Namespaces, and Pragmas

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using OpenCog.Attributes;
using OpenCog.Extensions;
using ImplicitFields = ProtoBuf.ImplicitFields;
using ProtoContract = ProtoBuf.ProtoContractAttribute;
using Serializable = System.SerializableAttribute;
using UnityEngine;
using OpenCog.Utility;

//using PostSharp.Aspects;
//using OpenCog.Aspects;

//The private field is assigned but its value is never used
#pragma warning disable 0414

#endregion

namespace OpenCog.Map
{

/// <summary>
/// The OpenCog OCMap.
/// </summary>
#region Class Attributes

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
[OCExposePropertyFields]
[Serializable]
[AddComponentMenu("VoxelEngine/Map")]

#endregion
public class OCMap : OCMonoBehaviour
{

	//---------------------------------------------------------------------------

	#region Private Member Data

	//---------------------------------------------------------------------------

	// TOFIX: SerializeField necessary here?
	[SerializeField]
	private OpenCog.BlockSet.OCBlockSet _blockSet;
	private List3D<OCChunk> _chunks = new List3D<OCChunk> ();
	private OpenCog.Map.Lighting.OCSunLightMap _sunLightmap = new OpenCog.Map.Lighting.OCSunLightMap ();
	private OpenCog.Map.Lighting.OCLightMap _lightmap = new OpenCog.Map.Lighting.OCLightMap ();

	private string _mapName;

	private int _floorHeight;

	private bool _chunkLimitsInitialized;
	private int _minChunkX;
	private int _minChunkY;
	private int _minChunkZ;
	private int _maxChunkX;
	private int _maxChunkY;
	private int _maxChunkZ;

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Accessors and Mutators

	//---------------------------------------------------------------------------

	/// <summary>
	/// Gets the number of chunks the map contains along the X axis.
	/// </summary>
	/// <value>
	/// The number of chunks the map contains along the X axis.
	/// </value>
	public int ChunkCountX
	{
		get
		{
			return (_maxChunkX - _minChunkX) + 1;
		}
	}

	/// <summary>
	/// Gets the number of chunks the map contains along the Y axis.
	/// </summary>
	/// <value>
	/// The number of chunks the map contains along the Y axis.
	/// </value>
	public int ChunkCountY
	{
		get
		{
			return (_maxChunkY - _minChunkY) + 1;
		}
	}

	/// <summary>
	/// Gets the number of chunks the map contains along the Z axis.
	/// </summary>
	/// <value>
	/// The number of chunks the map contains along the Z axis.
	/// </value>
	public int ChunkCountZ
	{
		get
		{
			return (_maxChunkZ - _minChunkZ) + 1;
		}
	}

	// The height of the floor level, OpenCog ignores blocks below this level. May need to use water level for this (if everything below
	// this level gets lake-ified) or use some method to calculate the lowest grass block.
	public int FloorHeight
	{
		get { return _floorHeight; }
	}

	/// <summary>
	/// Gets or sets the name of the map.
	/// </summary>
	/// <value>
	/// The name of the map.
	/// </value>
	public string MapName
	{
		get { return _mapName;}
		set { _mapName = Value;}
	}
			
	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Public Member Functions

	//---------------------------------------------------------------------------

	public void SetBlockAndRecompute (OCBlockData block, Vector3i pos)
	{
		MethodInfo info = this.GetType().GetMember("SetBlockAndRecompute")[0] as MethodInfo;

// TOFIX: uncomment when aspect stuff is in place?
//		object[] attributes = info.GetCustomAttributes(typeof(OCLogAspect), true);
//		OCLogAspect asp = null;
//
//		if(attributes != null)
//			asp = attributes[0] as OCLogAspect;
//
//		if(asp == null)
//			Debug.Log("No OCLog Aspect...");
//
//		asp.OnEntry(null);

		SetBlock (block, pos);

		// Convert the global coordinate of the block to the chunk coordinates.
		Vector3i chunkPos = OCChunk.ToChunkPosition (pos);

		UpdateChunkLimits(chunkPos);

		// Convert the global coordinate of the block to the coordinate within the chunk.
		Vector3i localPos = OCChunk.ToLocalPosition (pos);
		
		SetDirty (chunkPos);

		// If on the lower boundary of a chunk...set the neighbouring chunk to dirty too.
		if (localPos.x == 0)
			SetDirty (chunkPos - Vector3i.right);
		if (localPos.y == 0)
			SetDirty (chunkPos - Vector3i.up);
		if (localPos.z == 0)
			SetDirty (chunkPos - Vector3i.forward);

		// If on the upper boundary of a chunk...set the neighbouring chunk to dirty too.
		if (localPos.x == OCChunk.SIZE_X - 1)
			SetDirty (chunkPos + Vector3i.right);
		if (localPos.y == OCChunk.SIZE_Y - 1)
			SetDirty (chunkPos + Vector3i.up);
		if (localPos.z == OCChunk.SIZE_Z - 1)
			SetDirty (chunkPos + Vector3i.forward);
		
		OCSunLightComputer.RecomputeLightAtPosition (this, pos);
		OCLightComputer.RecomputeLightAtPosition (this, pos);
		
		UpdateMeshColliderAfterBlockChange ();
// TOFIX: uncomment when aspect stuff is in place?
//		asp.OnExit(null);
	}

	public bool IsPathOpen (UnityEngine.Transform characterTransform, float characterHeight, PathDirection intendedDirection)
	{
		bool bPathIsOpen = false;
		
		Vector3i vCharForward = VectorUtil.Vector3ToVector3i(characterTransform.forward);
		
		//Debug.Log ("vFeetPosition = [" + vFeetPosition.x + ", " + vFeetPosition.y + ", " + vFeetPosition.z + "]");
		//Debug.Log ("vFeetForwardPosition = [" + vFeetForwardPosition.x + ", " + vFeetForwardPosition.y + ", " + vFeetForwardPosition.z + "]");
		
		UnityEngine.Vector3 vFeet = new UnityEngine.Vector3 (characterTransform.position.x, characterTransform.position.y, characterTransform.position.z);
				
		vFeet.y -= (characterHeight / 2);
				
		UnityEngine.Vector3 vFeetForward = characterTransform.forward + vFeet;
		
		Vector3i viStandingOn = VectorUtil.Vector3ToVector3i(vFeet);
		//Debug.Log ("Standing on world block: [" + viStandingOn.x + ", " + viStandingOn.y + ", " + viStandingOn.z + "]");
		
		Vector3i viStandingOnForward = VectorUtil.Vector3ToVector3i(vFeetForward);
		//Debug.Log ("Forward of standing on world block: [" + viStandingOnForward.x + ", " + viStandingOnForward.y + ", " + viStandingOnForward.z + "]");
				
		Vector3i viLowerBody = new Vector3i (viStandingOn.x, viStandingOn.y, viStandingOn.z);
		viLowerBody += new Vector3i (0, 1, 0);
		//Debug.Log ("Lower body inhabits world block: [" + viLowerBody.x + ", " + viLowerBody.y + ", " + viLowerBody.z + "]");
		
		Vector3i viUpperBody = new Vector3i (viLowerBody.x, viLowerBody.y, viLowerBody.z);
		viUpperBody += new Vector3i (0, 1, 0);
		//Debug.Log ("Upper body inhabits world block: [" + viUpperBody.x + ", " + viUpperBody.y + ", " + viUpperBody.z + "]");
		
		// Prepare some block vectors to use later.
		Vector3i viOneAboveHead = viUpperBody + Vector3i.up; // The block direct above the upper body
		Vector3i viTwoAboveHead = viOneAboveHead + Vector3i.up; // The block two above the upper body
		
		Vector3i viForwardOneUnder = viStandingOnForward; // The block in front, one down
		Vector3i viForwardKneeHigh = viStandingOnForward + Vector3i.up; // The block in front of the lower body
		Vector3i viForwardChestHigh = viForwardKneeHigh + Vector3i.up; // The block in front of the upper body
		Vector3i viForwardOneAboveHead = viForwardChestHigh + Vector3i.up; // The block one above the block in front of the upper body
		Vector3i viForwardTwoAboveHead = viForwardOneAboveHead + Vector3i.up; // The block two above the block in front of the upper body
		
		Vector3i viTwoForwardKneeHigh = viForwardKneeHigh + vCharForward; // The block two steps ahead, in front of the lower body
		Vector3i viTwoForwardChestHigh = viForwardChestHigh + vCharForward; // The block two steps ahead, in front of the upper body
		
		Vector3i viThreeForwardKneeHigh = viTwoForwardKneeHigh + vCharForward; // The block three steps ahead, in front of the lower body
		Vector3i viThreeForwardChestHigh = viTwoForwardChestHigh + vCharForward; // The block three steps ahead, in front of the upper body
		
		Vector3i viTwoForwardOneUnder = viStandingOnForward + vCharForward; // The block two steps ahead, one down
		Vector3i viThreeForwardOneUnder = viTwoForwardOneUnder + vCharForward; // The block three steps ahead, one down
		
		//Debug.Log ("Forward knee high: [" + viForwardKneeHigh.x + ", " + viForwardKneeHigh.y + ", " + viForwardKneeHigh.z + "]");
		//Debug.Log ("Forward chest high: [" + viForwardChestHigh.x + ", " + viForwardChestHigh.y + ", " + viForwardChestHigh.z + "]");
		//Debug.Log ("Forward one under: [" + viForwardOneUnder.x + ", " + viForwardOneUnder.y + ", " + viForwardOneUnder.z + "]");
		
		
		//Debug.Log ("Forward lower block is: [" + viForwardKneeHigh.x + ", " + viForwardKneeHigh.y + ", " + viForwardKneeHigh.z + "]");
		//Debug.Log ("Forward upper block is: [" + viForwardChestHigh.x + ", " + viForwardChestHigh.y + ", " + viForwardChestHigh.z + "]");
		
		switch (intendedDirection) {
		case PathDirection.ForwardWalk:
			// Requires two clear blocks in front
			if (GetBlock (viForwardKneeHigh).IsEmpty () && GetBlock (viForwardChestHigh).IsEmpty ())
				// And one block under in front
				if (GetBlock (viForwardOneUnder).IsSolid ())
					bPathIsOpen = true;	
				break;
		case PathDirection.ForwardRun:
			// Requires two clear blocks for the next 3 forwards
			if (GetBlock (viForwardKneeHigh).IsEmpty() && GetBlock (viForwardChestHigh).IsEmpty())
				if (GetBlock (viTwoForwardKneeHigh).IsEmpty() && GetBlock (viTwoForwardChestHigh).IsEmpty())
					if (GetBlock (viThreeForwardKneeHigh).IsEmpty() && GetBlock (viThreeForwardChestHigh).IsEmpty())
						if (GetBlock (viForwardOneUnder).IsSolid () && GetBlock (viTwoForwardOneUnder).IsSolid () && GetBlock (viThreeForwardOneUnder).IsSolid ())
							bPathIsOpen = true;
			break;
		case PathDirection.ForwardClimb:
			// Requires a solid block lower front
			if (GetBlock (viForwardKneeHigh).IsSolid())
				// And two empty blocks above that
				if (GetBlock (viForwardChestHigh).IsEmpty () && GetBlock (viForwardOneAboveHead).IsEmpty ())
					bPathIsOpen = true;
			break;
		case PathDirection.ForwardDrop:
			// Requires 3 empty block in front, chest high, knee high and 1 underground
			if (GetBlock (viForwardKneeHigh).IsEmpty() && GetBlock (viForwardChestHigh).IsEmpty () && GetBlock (viForwardOneUnder).IsEmpty ())
				bPathIsOpen = true;
			break;
		case PathDirection.ForwardJump:
			// Requires two empty blocks above, and two empty blocks in front of those
			if (GetBlock (viOneAboveHead).IsEmpty () && GetBlock (viTwoAboveHead).IsEmpty () && GetBlock (viForwardOneAboveHead).IsEmpty () && GetBlock (viForwardTwoAboveHead).IsEmpty ())
				bPathIsOpen = true;
			break;
		default:
			Debug.Log ("Undefined PathDirection in IsPathOpen(basePosition, intendedDirection)");
			break;
		}
		
		//Debug.Log ("Test for PathDirection=" + intendedDirection.ToString () + " yields " + bPathIsOpen);
		
		return bPathIsOpen;
	}

	/// <summary>
	/// Sets the chunk to dirty.
	/// </summary>
	/// <param name='chunkPos'>
	/// Chunk position coordinates.
	/// </param>
	public void SetDirty (Vector3i chunkPos)
	{
		OCChunk chunk = GetChunk (chunkPos);
		if (chunk != null)
			chunk.GetChunkRendererInstance ().SetDirty ();
	}
	
	public void AddCollidersSync ()
	{
		Transform[] objects = GetComponentsInChildren<Transform> ();
 
		for (int i = 0; i < objects.Length; i++) {
			if (objects [i].gameObject.renderer) {
				Debug.Log ("We found us a " + objects [i].gameObject.GetType ().ToString ());
				
				MeshFilter myFilter = objects [i].gameObject.GetComponent<MeshFilter> ();
				MeshCollider myCollider = objects [i].gameObject.AddComponent<MeshCollider> ();
				
				myCollider.sharedMesh = null;
				
				myCollider.sharedMesh = myFilter.mesh;
				
				myCollider.enabled = true;
				
				Debug.Log ("i: " + objects [i].name);
				Debug.Log ("Center: " + myCollider.bounds.center.ToString ());
				Debug.Log ("Size: [" + myCollider.bounds.size.x + ", " + myCollider.bounds.size.y + ", " + myCollider.bounds.size.z + "]");
			}
		}
	}
	
	public void UpdateMeshColliderAfterBlockChange ()
	{
		StartCoroutine (StartUpdateMeshColliderAfterBlockChange ());	
	}

	public void AddColliders ()
	{
		StartCoroutine (StartAddColliders ());
	}

	public void SetBlock (OpenCog.BlockSet.BaseBlockSet.OCBlock block, Vector3i pos)
	{
		SetBlock (new OCBlockData (block), pos);
	}

	public void SetBlock (OpenCog.BlockSet.BaseBlockSet.OCBlock block, int x, int y, int z)
	{
		SetBlock (new OCBlockData (block), x, y, z);
	}
	
	public void SetBlock (OCBlockData block, Vector3i pos)
	{
		SetBlock (block, pos.x, pos.y, pos.z);
	}

	public void SetBlock (OCBlockData block, int x, int y, int z)
	{
		OCChunk chunk = GetChunkInstance (OCChunk.ToChunkPosition (x, y, z));
		if (chunk != null)
			chunk.SetBlock (block, OCChunk.ToLocalPosition (x, y, z));
	}
	
	public OCBlockData GetBlock (Vector3i pos)
	{
		return GetBlock (pos.x, pos.y, pos.z);
	}

	public OCBlockData GetBlock (int x, int y, int z)
	{
		OCChunk chunk = GetChunk (OCChunk.ToChunkPosition (x, y, z));
		if (chunk == null)
			return default(OCBlockData);
		return chunk.GetBlock (OCChunk.ToLocalPosition (x, y, z));
	}
	
	public int GetMaxY (int x, int z)
	{
		Vector3i chunkPos = OCChunk.ToChunkPosition (x, 0, z);
		chunkPos.y = chunks.GetMax ().y;
		Vector3i localPos = OCChunk.ToLocalPosition (x, 0, z);
		
		for (; chunkPos.y >= 0; chunkPos.y--) {
			localPos.y = OCChunk.SIZE_Y - 1;
			for (; localPos.y >= 0; localPos.y--) {
				OCChunk chunk = chunks.SafeGet (chunkPos);
				if (chunk == null)
					break;
				OCBlockData block = chunk.GetBlock (localPos);
				if (!block.IsEmpty ())
					return OCChunk.ToWorldPosition (chunkPos, localPos).y;
			}
		}
		
		return 0;
	}

		public OCChunk GetChunk (Vector3i chunkPos)
	{
		return chunks.SafeGet (chunkPos);
	}
	
	public List3D<OCChunk> GetChunks ()
	{
		return chunks;
	}
	
	public OpenCog.Map.Lighting.OCSunLightMap GetSunLightmap ()
	{
		return sunLightmap;
	}
	
	public OpenCog.Map.Lighting.OCLightMap GetLightmap ()
	{
		return lightmap;
	}
	
	public void SetBlockSet (OpenCog.BlockSet.OCBlockSet blockSet)
	{
		this.blockSet = blockSet;
	}

	public OpenCog.BlockSet.OCBlockSet GetBlockSet ()
	{
		return blockSet;
	}

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Private Member Functions

	//---------------------------------------------------------------------------

	/// <summary>
	/// Updates the chunk limits based on chunks that are added to the map
	/// </summary>
	/// <param name='chunkPosition'>
	/// The position of the chunk to update the chunk limits with.
	/// </param>
	private void UpdateChunkLimits(Vector3i chunkPosition)
	{
		if (_chunkLimitsInitialized)
		{
			_minChunkX = Mathf.Min(chunkPosition.x, _minChunkX);
			_minChunkY = Mathf.Min(chunkPosition.y, _minChunkY);
			_minChunkZ = Mathf.Min(chunkPosition.z, _minChunkZ);

			_maxChunkX = Mathf.Max(chunkPosition.x, _maxChunkX);
			_maxChunkY = Mathf.Max(chunkPosition.x, _maxChunkY);
			_maxChunkZ = Mathf.Max(chunkPosition.x, _maxChunkZ);
		}
		else
		{
			_minChunkX = chunkPosition.x;
			_minChunkY = chunkPosition.y;
			_minChunkZ = chunkPosition.z;

			_maxChunkX = chunkPosition.x;
			_maxChunkY = chunkPosition.y;
			_maxChunkZ = chunkPosition.z;
		}
	}

	private IEnumerator StartUpdateMeshColliderAfterBlockChange ()
	{
		Transform[] objects = GetComponentsInChildren<Transform> ();
		
		yield return null;
		
		for (int i = objects.Length -1; i >= 0; i--) {
			if (objects [i].gameObject.renderer) {
				MeshFilter myFilter = objects [i].gameObject.GetComponent<MeshFilter> ();
				MeshCollider myCollider = objects [i].gameObject.GetComponent<MeshCollider> ();

				if (myCollider != null) {

					myCollider.sharedMesh = null;
				
					myCollider.sharedMesh = myFilter.mesh;
				
					//Debug.Log ("Reapplied mesh for " + objects[i].gameObject.GetType ().ToString ());
				}
				
//				Debug.Log ("i: " + objects [i].name);
//				Debug.Log ("Center: " + myCollider.bounds.center.ToString());
//				Debug.Log ("Size: [" + myCollider.bounds.size.x + ", " + myCollider.bounds.size.y + ", " + myCollider.bounds.size.z + "]");
			}
		}
	}

	private IEnumerator StartAddColliders ()
	{
		Transform[] objects = GetComponentsInChildren<Transform> ();
		
		yield return null;
 
		for (int i = objects.Length -1; i >= 0; i--) {
			if (objects[i] != null && objects [i].gameObject.renderer) {
				//Debug.Log("We found us a " + objects[i].gameObject.GetType ().ToString ());
								
				if (objects[i].gameObject.GetComponent<MeshCollider>() == null)
				{
					MeshFilter myFilter = objects [i].gameObject.GetComponent<MeshFilter> ();
					MeshCollider myCollider = objects [i].gameObject.AddComponent<MeshCollider> ();
					
					myCollider.sharedMesh = null;

					myCollider.sharedMesh = myFilter.mesh;
					
					//Debug.Log ("i: " + objects [i].name);
					//Debug.Log ("Center: " + myCollider.bounds.center.ToString());
					//Debug.Log ("Size: [" + myCollider.bounds.size.x + ", " + myCollider.bounds.size.y + ", " + myCollider.bounds.size.z + "]");	
				}
			}
		}
	}


	private OCChunk GetChunkInstance (Vector3i chunkPos)
	{
		if (chunkPos.y < 0)
			return null;
		OCChunk chunk = GetChunk (chunkPos);
		if (chunk == null) {
			chunk = new OCChunk (this, chunkPos);
			chunks.AddOrReplace (chunk, chunkPos);
		}
		return chunk;
	}

//	private bool IsBlockOnChunkEdge (Vector3i blockPositionGlobal)
//	{
//		int chunkXDim = 16;
//		int chunkZDim = 16;
//		
//		if ((blockPositionGlobal.x == 0) || ((blockPositionGlobal.x + 1) % chunkXDim == 0))
//			return true;
//		else if ((blockPositionGlobal.z == 0) || ((blockPositionGlobal.z + 1) % chunkZDim == 0))
//			return true;
//		else 
//			return false;
//		
//	}
			
	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Other Members

	//---------------------------------------------------------------------------		

	public enum PathDirection
	{
		ForwardWalk,
		ForwardRun,
		ForwardClimb,
		ForwardJump,
		ForwardDrop
	};

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

}// class OCMap

}// namespace OpenCog



