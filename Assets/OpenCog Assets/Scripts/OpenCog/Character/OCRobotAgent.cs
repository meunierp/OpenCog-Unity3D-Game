
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

using System;
using System.Collections;
using Behave.Runtime;
using OpenCog.Actions;
using OpenCog.Attributes;
using OpenCog.Extensions;
using ProtoBuf;
using UnityEngine;
using Tree = Behave.Runtime.Tree;
using TreeType = BLOpenCogCharacterBehaviours.TreeType;
using ContextType = BLOpenCogCharacterBehaviours.ContextType;

namespace OpenCog
{

namespace Character
{

/// <summary>
/// The OpenCog OCRobotAgent.
/// </summary>
#region Class Attributes

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
[OCExposePropertyFields]
[Serializable]
#endregion
public class OCRobotAgent : OCMonoBehaviour, IAgent
{

	//---------------------------------------------------------------------------

	#region Private Member Data

	//---------------------------------------------------------------------------

	private Tree m_Tree;

	private Hashtable m_IdleParams;

	private Vector3i m_TargetBlockPos = Vector3i.zero;

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Accessors and Mutators

	//---------------------------------------------------------------------------

	public Vector3i TargetBlockPos
	{
		get {return m_TargetBlockPos;}
		set {m_TargetBlockPos = value;}
	}
			
	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------	

	#region Constructors

	//---------------------------------------------------------------------------
		
	/// <summary>
	/// Initializes a new instance of the <see cref="OpenCog.OCRobotAgent"/> class.
	/// Generally, intitialization should occur in the Start function.
	/// </summary>
	public OCRobotAgent()
	{
	}			

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Public Member Functions

	//---------------------------------------------------------------------------

	public IEnumerator Start()
	{
			m_Tree =
				BLOpenCogCharacterBehaviours.InstantiateTree
				( TreeType.CharacterBehaviours_RobotExploreBehaviour
				, this
				)
			;

			while(Application.isPlaying && m_Tree != null)
			{
				yield return new WaitForSeconds (1.0f / m_Tree.Frequency);
				UpdateAI();
			}


	}

	public void Update()
	{
//		if(Time.frameCount%120 == 0)
//		{
//			var bestWeight = -1.0;
//			String playing = "";
//			foreach (AnimationState s in animation)
//			{
//    		if (s.enabled && s.weight > bestWeight)
//				{
//       		playing += s.name + " ";
//        	bestWeight = s.weight;
//    		}
//			}
//			Debug.Log("Animation State: " + playing);
//		}
	}

	public BehaveResult	 Tick (Tree sender, bool init)
	{
//			Debug.Log
//			(
//				"Got ticked by unhandled " + (BLOpenCogCharacterBehaviours.IsAction( sender.ActiveID ) ? "action" : "decorator")
//			+ ( BLOpenCogCharacterBehaviours.IsAction( sender.ActiveID )
//				? ((BLOpenCogCharacterBehaviours.ActionType)sender.ActiveID).ToString()
//				: ((BLOpenCogCharacterBehaviours.DecoratorType)sender.ActiveID).ToString()
//				)
//			);

			return BehaveResult.Failure;
	}

//	public BehaveResult TickIdleAction(Tree sender, string stringParameter, float floatParameter, IAgent agent, object data)
//	{
//			Debug.Log("In Robot Idle...");
//
//			return BehaveResult.Success;
//	}

	public BehaveResult IdleAction
	{
			// tick handler
			get
			{
				OCIdleAction action = gameObject.GetComponent<OCIdleAction>();
				CharacterController charController = gameObject.GetComponent<CharacterController>();

				BehaveResult ret = DefaultActionTickHandler(action);

				if(ret != BehaveResult.Success)
					return ret;

				if(TargetBlockPos == Vector3i.zero  && charController.isGrounded)
				{
					action.Execute();
					//Debug.Log("In OCRobotAgent.IdleAction, " + action.GetType() + " Success");
					return BehaveResult.Success;
				}

				//Debug.Log("In OCRobotAgent.IdleAction, " + action.GetType() + " Failure");
				return BehaveResult.Failure;
			}

			// reset handler
			set
			{
			}
	}

	public BehaveResult ClimbAction
	{
			// tick handler
			get
			{
				OCClimbUpAction action = gameObject.GetComponent<OCClimbUpAction>();

				BehaveResult ret = DefaultActionTickHandler(action);

				if(ret != BehaveResult.Success)
					return ret;

				CharacterController charController = gameObject.GetComponent<CharacterController>();

				Vector3 robotPos = gameObject.transform.position;
				Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
				float robotUpDistance = Vector3.Dot(distanceVec, gameObject.transform.up);
				float robotForwardDistance = Vector3.Dot(distanceVec, gameObject.transform.forward);

				if(  TargetBlockPos != Vector3i.zero
					&& robotUpDistance >= 2.5f
					&& robotForwardDistance >= 0.5f
					&& charController.isGrounded)
				{
					action.Execute();
					//Debug.Log("In OCRobotAgent.ClimbAction, " + action.GetType() + " Success");
					return BehaveResult.Success;
				}

				//Debug.Log("In OCRobotAgent.ClimbAction, " + action.GetType() + " Failure");
				return BehaveResult.Failure;
			}

			// reset handler
			set
			{
			}
	}

	public BehaveResult RunAction
	{
			// tick handler
			get
			{
				OCRunForwardAction action = gameObject.GetComponent<OCRunForwardAction>();

				BehaveResult ret = DefaultActionTickHandler(action);

				if(ret != BehaveResult.Success)
					return ret;

				CharacterController charController = gameObject.GetComponent<CharacterController>();

				Vector3 robotPos = gameObject.transform.position;
				Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
				float robotForwardDistance = Vector3.Dot(distanceVec, gameObject.transform.forward);
				float robotUpDistance = Vector3.Dot(distanceVec, gameObject.transform.up);
				float robotRightDistance = Vector3.Dot(distanceVec, gameObject.transform.right);
				float robotLeftDistance = Vector3.Dot(distanceVec, -gameObject.transform.right);

				if(	 robotForwardDistance > 2.5f
//					&& robotRightDistance < 1.5f
//					&& robotLeftDistance < 1.5f
					&& charController.isGrounded
					)
				{
					action.Execute();
					Debug.Log("In OCRobotAgent.RunAction, " + action.GetType() + " Success");
					return BehaveResult.Success;
				}

				Debug.Log("In OCRobotAgent.RunAction, " + action.GetType() + " Failure");
				return BehaveResult.Failure;
			}

			// reset handler
			set
			{
			}
	}

	public BehaveResult JumpAction
	{
			// tick handler
			get
			{
				OCJumpUpAction action = gameObject.GetComponent<OCJumpUpAction>();

				BehaveResult ret = DefaultActionTickHandler(action);

				if(ret != BehaveResult.Success)
					return ret;

				CharacterController charController = gameObject.GetComponent<CharacterController>();

				Vector3 robotPos = gameObject.transform.position;
				Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
				float robotUpDistance = Vector3.Dot(distanceVec, gameObject.transform.up);

				if(	TargetBlockPos != Vector3i.zero
					&& robotUpDistance >= 2.5f
					&& charController.isGrounded)
				{
					action.Execute();
					//Debug.Log("In OCRobotAgent.JumpAction, " + action.GetType() + " Success");
					return BehaveResult.Success;
				}

				//Debug.Log("In OCRobotAgent.JumpAction, " + action.GetType() + " Failure");
				return BehaveResult.Failure;
			}

			// reset handler
			set
			{
			}
	}

	public BehaveResult TurnLeftAction
	{
			// tick handler
			get
			{
				OCTurnLeftAction action = gameObject.GetComponent<OCTurnLeftAction>();

				BehaveResult ret = DefaultActionTickHandler(action);

				if(ret != BehaveResult.Success)
					return ret;

				CharacterController charController = gameObject.GetComponent<CharacterController>();

				Vector3 robotPos = gameObject.transform.position;
				Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
				float robotForwardDistance = Vector3.Dot(distanceVec, gameObject.transform.forward);
				float robotRightDistance = Vector3.Dot(distanceVec, gameObject.transform.right);
				float robotLeftDistance = Vector3.Dot(distanceVec, -gameObject.transform.right);

				if(TargetBlockPos != Vector3i.zero && robotLeftDistance >= 0.5f && charController.isGrounded)
				{
					action.Execute();
					//Debug.Log("In OCRobotAgent.TurnLeftAction, " + action.GetType() + " Success");
					return BehaveResult.Success;
				}

				//Debug.Log("In OCRobotAgent.TurnLeftAction, " + action.GetType() + " Failure");
				return BehaveResult.Failure;
			}

			// reset handler
			set
			{
			}
	}

	public BehaveResult TurnRightAction
	{
			// tick handler
			get
			{
				OCTurnRightAction action = gameObject.GetComponent<OCTurnRightAction>();

				BehaveResult ret = DefaultActionTickHandler(action);

				if(ret != BehaveResult.Success)
					return ret;

				CharacterController charController = gameObject.GetComponent<CharacterController>();

				Vector3 robotPos = gameObject.transform.position;
				Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
				float robotForwardDistance = Vector3.Dot(distanceVec, gameObject.transform.forward);
				float robotRightDistance = Vector3.Dot(distanceVec, gameObject.transform.right);
				float robotLeftDistance = Vector3.Dot(distanceVec, -gameObject.transform.right);

				if(TargetBlockPos != Vector3i.zero && robotRightDistance >= 0.5f && charController.isGrounded)
				{
					action.Execute();
					//Debug.Log("In OCRobotAgent.TurnRightAction, " + action.GetType() + " Success");
					return BehaveResult.Success;
				}

				//Debug.Log("In OCRobotAgent.TurnRightAction, " + action.GetType() + " Failure");
				return BehaveResult.Failure;
			}

			// reset handler
			set
			{
			}
	}

	public BehaveResult WalkAction
	{
			// tick handler
			get
			{
				OCWalkForwardAction action = gameObject.GetComponent<OCWalkForwardAction>();

				BehaveResult ret = DefaultActionTickHandler(action);

				if(ret != BehaveResult.Success)
					return ret;

				CharacterController charController = gameObject.GetComponent<CharacterController>();

				Vector3 robotPos = gameObject.transform.position;
				Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;
				float robotForwardDistance = Vector3.Dot(distanceVec, gameObject.transform.forward);
				float robotUpDistance = Vector3.Dot(distanceVec, gameObject.transform.up);
				float robotRightDistance = Vector3.Dot(distanceVec, gameObject.transform.right);
				float robotLeftDistance = Vector3.Dot(distanceVec, -gameObject.transform.right);

				if(  TargetBlockPos != Vector3i.zero
					&& robotForwardDistance <= 2.5f
					&& robotForwardDistance >= 0.5f
//					&& robotRightDistance < 0.5f
//					&& robotLeftDistance < 0.5f
					&& charController.isGrounded
					)
				{
					action.Execute();
					//Debug.Log("In OCRobotAgent.WalkAction, " + action.GetType() + " Success");
					return BehaveResult.Success;
				}

				//Debug.Log("In OCRobotAgent.WalkAction, " + action.GetType() + " Failure");
				return BehaveResult.Failure;
			}

			// reset handler
			set
			{
			}
	}

	public void UpdateAI()
	{
		m_Tree.Tick();

		Map map = (Map)GameObject.FindObjectOfType(typeof(Map));

		List3D<Chunk> chunks = map.GetChunks();

		Vector3 robotPos = gameObject.transform.position;
		Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;

//		if(distanceVec.y < -1.0f + 0.5f && distanceVec.y > -1.0f - 0.5f)
		if(distanceVec.sqrMagnitude < 1.75f)
		{
			Debug.Log("We've arrived at our goal TNT block...");
			map.SetBlockAndRecompute(new BlockData(), TargetBlockPos);
			TargetBlockPos = Vector3i.zero;
		}

		bool doesTNTExist = false;

		//distanceVec = new Vector3(1000,1000,1000);
		for(int cx=0; cx<chunks.GetMaxX(); ++cx)
		{
			for(int cy=0; cy<chunks.GetMaxY(); ++cy)
			{
				for(int cz=0; cz<chunks.GetMaxZ(); ++cz)
				{
					Vector3i chunkPos = new Vector3i(cx,cy,cz);
					Chunk chunk = chunks.SafeGet(chunkPos);
					if(chunk != null)
					{
						for(int z=0; z<Chunk.SIZE_Z; z++)
						{
							for(int x=0; x<Chunk.SIZE_X; x++)
							{
								for(int y=0; y<Chunk.SIZE_Y; y++)
								{
									Vector3i localPos = new Vector3i(x,y,z);
									BlockData blockData = chunk.GetBlock(localPos);
									Vector3i candidatePos = Chunk.ToWorldPosition(chunk.GetPosition(), localPos);
									Vector3 candidateVec = ((Vector3)candidatePos) - robotPos;
									if(!blockData.IsEmpty() && blockData.block.GetName() == "TNT")
									{
										doesTNTExist = true;
										if(candidateVec.sqrMagnitude < distanceVec.sqrMagnitude)
										{
											TargetBlockPos = candidatePos;
											distanceVec = candidateVec;
											Debug.Log("We found some TNT nearby: " + TargetBlockPos + "!");
										}
									}
								}
							}
						}
					}
				}
			}
		}

		if(!doesTNTExist && TargetBlockPos != Vector3i.zero)
		{
			Debug.Log("No more TNT... :(");
			TargetBlockPos = Vector3i.zero;
		}
	}

	public void	 Reset (Tree sender)
	{
	}

	public int	 SelectTopPriority (Tree sender, params int[] IDs)
	{
			return IDs[0];
	}

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Private Member Functions

	//---------------------------------------------------------------------------

	private BehaveResult DefaultActionTickHandler(OCAction action)
	{
		Vector3 robotPos = gameObject.transform.position;
		Vector3 distanceVec = ((Vector3)TargetBlockPos) - robotPos;

		if(action.ShouldTerminate())
		{
			//action.Terminate();
			Debug.Log("In OCRobotAgent.DefaultActionTickHandler, " + action.GetType() + " Failure");
			return BehaveResult.Failure;
		}

		if(action.IsExecuting())
		{
			//Debug.Log("In OCRobotAgent.DefaultActionTickHandler, " + action.GetType() + " Running");
			return BehaveResult.Running;
		}

		if(TargetBlockPos != Vector3i.zero)
		{
			//Debug.Log("In OCRobotAgent.DefaultActionTickHandler, Distance to TNT block is: " + distanceVec.magnitude + ", Vector is: " + distanceVec);
		}

		return BehaveResult.Success;
	}
			
	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Member Classes

	//---------------------------------------------------------------------------		

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

}// class OCRobotAgent

}// namespace Character

}// namespace OpenCog




