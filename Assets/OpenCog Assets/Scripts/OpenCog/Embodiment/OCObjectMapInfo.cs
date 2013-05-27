
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
using OpenCog.Attributes;
using OpenCog.Extensions;
using ImplicitFields = ProtoBuf.ImplicitFields;
using ProtoContract = ProtoBuf.ProtoContractAttribute;
using Serializable = System.SerializableAttribute;

//The private field is assigned but its value is never used
#pragma warning disable 0414

#endregion

namespace OpenCog.Embodiment
{

/// <summary>
/// The OpenCog OCObjectMapInfo.
/// </summary>
#region Class Attributes
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
[OCExposePropertyFields]
[Serializable]
	
#endregion
public class OCObjectMapInfo : OCMonoBehaviour
	{

		//---------------------------------------------------------------------------

	#region Private Member Data

		//---------------------------------------------------------------------------
		
		/// <summary>
		/// An example variable.  Don't fall into the trap of making all variables
		/// public (I know Unity encourages you to do this).  Instead, make use of
		/// public properties whenever possible.
		/// </summary>
		private int _exampleVar;
		
		 #region Constants
		private static readonly float POSITION_DISTANCE_THRESHOLD = 0.05f;
		private static readonly float ROTATION_DELTA = 0.0001f;
		private static readonly float DEFAULT_AVATAR_LENGTH = 1f;
		private static readonly float DEFAULT_AVATAR_WIDTH = 1f;
		private static readonly float DEFAULT_AVATAR_HEIGHT = 3f;


        #endregion
		
		private string _id;
		private string _name;
		private string _type;
		private UnityEngine.Vector3 _position; // Position of object
		private Vector3Wrapper _positionWrapper;
		private Utility.Rotation _rotation = new Utility.Rotation(0, 0, 0); // Rotation of object
		private UnityEngine.Vector3 _velocity; // Velocity of an object, if it is moving.
		private Vector3Wrapper _velocityWrapper;
		private float _length, _width, _height; // Size of an object.
		private float _weight; // weight of an object
		private UnityEngine.Vector3 _startMovePos; // the lastest time start to move position
		private Dictionary<string, Embodiment.OCTag> _tags = new Dictionary<string, Embodiment.OCTag>();
		private VISIBLE_STATUS _visibility = VISIBLE_STATUS.VISIBLE; // Set the visibility of an object to visible by default.

		//---------------------------------------------------------------------------

	#endregion

		//---------------------------------------------------------------------------

	#region Accessors and Mutators

		//---------------------------------------------------------------------------
		
		public string ID {
			get { return _id; }
			set { _id = value; }
		}

		public string Name {
			get { return _name; }
			set { _name = value; }
		}

		public string Type {
			get { return _type; }
			set { _type = value; }
		}

		// Will be called implicitly when serialized by protobuf-net.
		// Do not invoke this explicitly.
		public Vector3Wrapper PositionWrapper {
			get { return _positionWrapper; }
			set { Position = value.ToVector3 (); }
		}

		public UnityEngine.Vector3 Position {
			get { return _position; }
			set {
				_position = value;
				_positionWrapper = new Vector3Wrapper (_position);
			}
		}

		// Will be called implicitly when serialized by protobuf-net.
		// Do not invoke this explicitly.
		public Vector3Wrapper VelocityWrapper {
			get { return _velocityWrapper; }
			set { Velocity = value.ToVector3 ();  }
		}

		public UnityEngine.Vector3 Velocity {
			get { return _velocity; }
			set {
				_velocity = value;
				_velocityWrapper = new Vector3Wrapper (_velocity);
			}
		}

		public Utility.Rotation Rotation {
			get { return _rotation; }
			set { _rotation = value; }
		}

		public float Length {
			get { return _length; }
			set { _length = value; }
		}

		public float Width {
			get { return _width; }
			set { _width = value; }
		}

		public float Height {
			get { return _height; }
			set { _height = value; }
		}

		public VISIBLE_STATUS Visibility {
			get { return _visibility; }
			set { _visibility = value; }
		}

		public Dictionary<string, Embodiment.OCTag> Properties {
			get { return _tags; }
			set { _tags = value; }
		}

		public float Weight {
			get { return _weight; }
			set { _weight = value; }
		}

		public UnityEngine.Vector3 Size {
			get { return new UnityEngine.Vector3 ((float)this._length, (float)this._width, (float)this._height); }
		}

		//---------------------------------------------------------------------------

	#endregion

		//---------------------------------------------------------------------------

	#region Public Member Functions

		//---------------------------------------------------------------------------

		/// <summary>
		/// Called when the script instance is being loaded.
		/// </summary>
		public void Awake ()
		{
			Initialize ();
			OCLogger.Fine (gameObject.name + " is awake.");
		}

		/// <summary>
		/// Use this for initialization
		/// </summary>
		public void Start ()
		{
			OCLogger.Fine (gameObject.name + " is started.");
		}

		/// <summary>
		/// Update is called once per frame.
		/// </summary>
		public void Update ()
		{
			OCLogger.Fine (gameObject.name + " is updated.");	
		}
		
		/// <summary>
		/// Reset this instance to its default values.
		/// </summary>
		public void Reset ()
		{
			Uninitialize ();
			Initialize ();
			OCLogger.Fine (gameObject.name + " is reset.");	
		}

		/// <summary>
		/// Raises the enable event when OCObjectMapInfo is loaded.
		/// </summary>
		public void OnEnable ()
		{
			OCLogger.Fine (gameObject.name + " is enabled.");
		}

		/// <summary>
		/// Raises the disable event when OCObjectMapInfo goes out of scope.
		/// </summary>
		public void OnDisable ()
		{
			OCLogger.Fine (gameObject.name + " is disabled.");
		}

		/// <summary>
		/// Raises the destroy event when OCObjectMapInfo is about to be destroyed.
		/// </summary>
		public void OnDestroy ()
		{
			Uninitialize ();
			OCLogger.Fine (gameObject.name + " is about to be destroyed.");
		}
		
		public Embodiment.OCTag CheckTagExists (string keyStr)
		{
			Embodiment.OCTag tagToCheck = _tags[keyStr];

			return tagToCheck;
		}

		public void AddTag (string keyStr, string valueStr, System.Type type)
		{
			// Check if property existing
			Embodiment.OCTag tagToAdd = CheckTagExists(keyStr);

			if (_tags.ContainsKey(keyStr)) {
				_tags.Remove (keyStr);
			}

			_tags.Add(keyStr, new OCTag(valueStr, type));
		}

		public void RemoveTag (string keyStr)
		{
			// Check if property existing
			if (_tags.ContainsKey(keyStr)) {
				_tags.Remove (keyStr);
			}
		}

		public void UpdateTag (string keyStr, string valueStr, System.Type type)
		{
			AddTag (keyStr, valueStr, type);
		}

		//---------------------------------------------------------------------------

	#endregion

		//---------------------------------------------------------------------------

	#region Private Member Functions

		//---------------------------------------------------------------------------
	
		/// <summary>
		/// Initializes this instance.  Set default values here.
		/// </summary>
		private void Initialize ()
		{

		}
	
		/// <summary>
		/// Uninitializes this instance.  Cleanup refernces here.
		/// </summary>
		private void Uninitialize ()
		{
		}
			
		//---------------------------------------------------------------------------

	#endregion

		//---------------------------------------------------------------------------

	#region Other Members

		//---------------------------------------------------------------------------		

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenCog.OCObjectMapInfo"/> class.  
		/// Generally, intitialization should occur in the Start or Awake
		/// functions, not here.
		/// </summary>
		public OCObjectMapInfo ()
		{
		}
		
		public OCObjectMapInfo (UnityEngine.GameObject gameObject)
		{
			// Get id of a game object
			_id = gameObject.GetInstanceID ().ToString ();
			// Get name
			_name = gameObject.name;
			// TODO: By default, we are using object type.
			_type = OCEmbodimentXMLTags.ORDINARY_OBJECT_TYPE;

			// Convert from unity coordinate to OAC coordinate.
			_position = Utility.VectorUtil.ConvertToOpenCogCoord (gameObject.transform.position);
			// Get rotation
			_rotation = new Utility.Rotation (gameObject.transform.rotation);
			// Calculate the velocity later
			_velocity = UnityEngine.Vector3.zero;

			// Get size
			if (gameObject.collider != null) {
				// Get size information from collider.
				_width = gameObject.collider.bounds.size.z;
				_height = gameObject.collider.bounds.size.y;
				_length = gameObject.collider.bounds.size.x;
			} else {
				OCLogger.Warn ("No collider for gameobject " + gameObject.name + ", assuming a point.");

				// Set default value of the size.
				_width = 0.1f;
				_height = 0.1f;
				_length = 0.1f;
			}

			if (gameObject.tag == "OCA") {
				// This is an OC avatar, we will use the brain id instead of unity id.
				OpenCog.Embodiment.OCConnector connector = gameObject.GetComponent<OpenCog.Embodiment.OCConnector> () as OpenCog.Embodiment.OCConnector;

				if (connector != null)
					_id = connector.BrainID;
				_type = OCEmbodimentXMLTags.PET_OBJECT_TYPE;

			} else if (gameObject.tag == "Player") {
				// This is a human player avatar.
				_type = OCEmbodimentXMLTags.AVATAR_OBJECT_TYPE;
				_length = OCObjectMapInfo.DEFAULT_AVATAR_LENGTH;
				_width = OCObjectMapInfo.DEFAULT_AVATAR_WIDTH;
				_height = OCObjectMapInfo.DEFAULT_AVATAR_HEIGHT;
			}

			// Get weight
			if (gameObject.rigidbody != null) {
				_weight = gameObject.rigidbody.mass;
			} else {
				_weight = 0.0f;
			}

			// Get a property manager instance
			// TOFIX
//			OCPropertyManager manager = gameObject.GetComponent<OCPropertyManager> () as OCPropertyManager;
//			if (manager != null) {
//				// Copy all OC properties from the manager, if any.
//				foreach (OpenCog.Serialization.OCPropertyField ocp in manager.propertyList) {
//					this.AddProperty (ocp.Key, ocp.value, ocp.valueType);
//				}
//			}

			this.AddTag ("visibility-status", "visible", System.Type.GetType("System.String"));
			this.AddTag ("detector", "true", System.Type.GetType("System.Boolean"));
			
			string gameObjectName = gameObject.name;
			if (gameObjectName.Contains ("("))
				gameObjectName = gameObjectName.Remove (gameObjectName.IndexOf ('('));

			this.AddTag ("class", gameObjectName, System.Type.GetType("System.String"));
		}

		public static OCObjectMapInfo CreateObjectMapInfo(int chunkX, int chunkY, int chunkZ, int blockGlobalX, int blockGlobalY, int blockGlobalZ, OpenCog.Map.OCBlockData blockData)
		{
			string blockName = "BLOCK_" + blockData.GetHashCode();
			OCObjectMapInfo mapinfo = new OCObjectMapInfo ();
			mapinfo.Height = 1;
			mapinfo.Width = 1;
			mapinfo.Length = 1;
			mapinfo.Type = OCEmbodimentXMLTags.STRUCTURE_OBJECT_TYPE;
			mapinfo.ID = blockName;
			mapinfo.Name = blockName;
			mapinfo.Velocity = UnityEngine.Vector3.zero;
			mapinfo.Position = new UnityEngine.Vector3(blockGlobalX, blockGlobalY, blockGlobalZ);

			// Add block properties
			mapinfo.AddTag ("class", "block", System.Type.GetType("System.String"));
			mapinfo.AddTag ("visibility-status", "visible", System.Type.GetType("System.String"));
			mapinfo.AddTag ("detector", "true", System.Type.GetType("System.Boolean"));
			mapinfo.AddTag (OCEmbodimentXMLTags.MATERIAL_ATTRIBUTE, blockData.GetType().ToString(), System.Type.GetType("System.String"));
			//mapinfo.AddProperty("color_name", "green", PropertyType.STRING);
			return mapinfo;

		}
		
//		public static OCObjectMapInfo CreateTerrainMapInfo (Chunk chunk, uint x, uint y, uint z, BlockData blockData)
//		{
//
//			// Construct a unique block name.
//			string blockName = "BLOCK_" + x + "_" + y + "_" + z;
//			OCObjectMapInfo mapinfo = new OCObjectMapInfo ();
//			mapinfo.Height = 1;
//			mapinfo.Width = 1;
//			mapinfo.Length = 1;
//			mapinfo.Type = OCEmbodimentXMLTags.STRUCTURE_OBJECT_TYPE;
//			mapinfo.Id = blockName;
//			mapinfo.Name = blockName;
//			UnityEngine.Vector3 pos = new UnityEngine.Vector3 (chunk.X * chunk.Width + x, chunk.Z * chunk.Depth + z, chunk.Y * chunk.Height + y);
//			pos = VectorUtil.ConvertToCentralCoord (pos, mapinfo.Size);
//			mapinfo.Velocity = UnityEngine.Vector3.zero;
//			mapinfo.Position = pos;
//			Utility.Rotation rot = new Utility.Rotation (0, 0, 0);
//			mapinfo.Rotation = rot;
//
//			// Add block properties
//			mapinfo.AddProperty ("class", "block", System.Type.GetType("System.String"));
//			mapinfo.AddProperty ("visibility-status", "visible", System.Type.GetType("System.String"));
//			mapinfo.AddProperty ("detector", "true", System.Type.GetType("System.Boolean"));
//			mapinfo.AddProperty (OCEmbodimentXMLTags.MATERIAL_ATTRIBUTE, blockData.GetType().ToString(), PropertyType.STRING);
//			//mapinfo.AddProperty("color_name", "green", PropertyType.STRING);
//			return mapinfo;
//		}
		
		/// <summary>
		/// Since protobuf-net requires a data contract declaration on fields of 
		/// class that are to be serialized, we need to wrap the built-in types of
		/// Unity3D with manual data contract declaration.
		/// </summary>
	    public class Vector3Wrapper
		{
			public float x;
			public float y;
			public float z;

			// Constructor from a Unity3D Vector3
			public Vector3Wrapper (UnityEngine.Vector3 vec)
			{
				x = vec.x;
				y = vec.y;
				z = vec.z;
			}

			// Dummy constructor
			public Vector3Wrapper ()
			{
				x = 0.0f;
				y = 0.0f;
				z = 0.0f;
			}

			public UnityEngine.Vector3 ToVector3 ()
			{
				return new UnityEngine.Vector3 (x, y, z);
			}
		}

		public enum VISIBLE_STATUS
		{
			VISIBLE = 0,
			INVISIBLE = 1,
			UNKNOWN = 2
		};

		//---------------------------------------------------------------------------

	#endregion

		//---------------------------------------------------------------------------

	}// class OCObjectMapInfo

}// namespace OpenCog



