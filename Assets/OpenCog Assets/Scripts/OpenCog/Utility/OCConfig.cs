
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
using System.IO;
using System.Net;
using OpenCog.Attributes;
using OpenCog.Extensions;
using OpenCog.Utility;
using ProtoBuf;
using UnityEngine;

namespace OpenCog
{

/// <summary>
/// The OpenCog OCConfig.
/// </summary>
#region Class Attributes

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
[OCExposePropertyFields]
[Serializable]
#endregion
public class OCConfig : OCSingletonScriptableObject< OCConfig >
{

	//---------------------------------------------------------------------------

	#region Private Member Data

	//---------------------------------------------------------------------------

	protected static Hashtable m_settings = new Hashtable();

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Accessors and Mutators

	//---------------------------------------------------------------------------


			
	//---------------------------------------------------------------------------

	#endregion
		
	//---------------------------------------------------------------------------

	#region Public Member Functions

	//---------------------------------------------------------------------------
		
	/// <summary>
	/// Raises the enable event for the OCConfig.
	/// </summary>
	public void OnEnable()
	{
		m_settings["GENERATE_TICK_MESSAGE"] = "true";

		// Proxy config
		m_settings["MY_ID"] = "PROXY";
		m_settings["MY_IP"] = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
		m_settings["MY_PORT"] = "16315";
		
		// Router config
		m_settings["ROUTER_ID"] = "ROUTER";
		m_settings["ROUTER_IP"] = "192.168.1.48";
		m_settings["ROUTER_PORT"] = "16312";
		
		// Spawner config
		m_settings["SPAWNER_ID"] = "SPAWNER";
		
		// Unread messages management
		m_settings["UNREAD_MESSAGES_CHECK_INTERVAL"] = "10";
		m_settings["UNREAD_MESSAGES_RETRIEVAL_LIMIT"] = "1";
		m_settings["NO_ACK_MESSAGES"] = "true";
		
		// Time for sleeping in server loop
		m_settings["SERVER_LOOP_SLEEP_TIME"] = "100";
		
		// Time interval for sending perception (map-info, physiological) messages in milliseconds
		m_settings["MESSAGE_SENDING_INTERVAL"] = "100";
		// Interval between time ticks (in milliseconds)
		m_settings["TICK_INTERVAL"] = "500";
		
		// Number of simulated milliseconds per tick 
		// (Default value == TICK_INTERVAL) => (simulated time == real time)
		// For accelerating the simulated time (useful for automated tests), 
		// this value must/may be increased. 
		m_settings["MILLISECONDS_PER_TICK"] = "500";
		
		// Parameters for controlling Physiological feelings:
		// 480 = 60/3 * 24 means the eat action is expected to happen once per 3 minute 
		// when the virtual pet does nothing else
		m_settings["EAT_STOPS_PER_DAY"] = "480";
		m_settings["DRINK_STOPS_PER_DAY"] = "480";
		m_settings["PEE_STOPS_PER_DAY"] = "480";
		m_settings["POO_STOPS_PER_DAY"] = "480";
		m_settings["MAX_ACTION_NUM"] = "50";  // Maximum number of actions the avatar can do without eating
		m_settings["EAT_ENERGY_INCREASE"] = "0.55";
		//		m_settings["AT_HOME_DISTANCE"] = "3.8";  // Avatar is at home, if the distance between avatar and home is smalled than this value
		//		m_settings["FITNESS_INCREASE_AT_HOME"] = "0.008333"; // Equals 1/(60/0.5), need 60 seconds at most to increase to 1
		m_settings["FITNESS_DECREASE_OUTSIDE_HOME"] = "0.005556";  // Equals 1/(60*1.5/0.5), need 1.5 minutes at most to decrease to 0
		m_settings["POO_INCREASE"] = "0.05";
		m_settings["DRINK_THIRST_DECREASE"] = "0.15";
		m_settings["DRINK_PEE_INCREASE"] = "0.05";	
		m_settings["INIT_ENERGY"] = "1.0"; 
		m_settings["INIT_FITNESS"] = "0.80"; 
		
		// Interval between messages sending (in milliseconds)
		m_settings["MESSAGE_SENDING_INTERVAL"] = "100";
		
		// Map min/max position
		m_settings["GLOBAL_POSITION_X"] = "500";	//"-165000";
		m_settings["GLOBAL_POSITION_Y"] = "500";	//"-270000";
		m_settings["AVATAR_VISION_RADIUS"] = "200";	//"64000";
		
		// Golden standard generation
		m_settings["GENERATE_GOLD_STANDARD"] = "true";
		// filename where golden standard message will be recorded 
		m_settings["GOLD_STANDARD_FILENAME"] = "GoldStandards.txt";
		
		m_settings["AVATAR_STORAGE_URL"] = ""; // Default is Jack's appearance
		
		// There are six levels: NONE, ERROR, WARN, INFO, DEBUG, FINE.
		m_settings["LOG_LEVEL"] = "DEBUG";
		
		// OpenCog properties persistence data file
		m_settings["OCPROPERTY_DATA_FILE"] = ".\\oc_properties.dat";			
	}
		
	/// <summary>
	/// Load passed file and redefines values for parameters.
	/// Parameters which are not mentioned in the file will keep their default value.
	/// Parameters which do not have default values are discarded.
	/// </summary>
	/// <param name='fileName'>
	/// The config file's name.
	/// </param>
	public void LoadFromFile(string fileName)
	{
		StreamReader reader = new StreamReader(fileName);
    char[] separator = {'=',' '};
      int linenumber = 0;
    
		while (!reader.EndOfStream)
		{
      string line = reader.ReadLine();
        linenumber++;
      
      // not a commentary or an empty line
      if(line.Length > 0 && line[0] != '#'){
          string[] tokens = line.Split(separator,StringSplitOptions.RemoveEmptyEntries);
          if (tokens.Length < 2)
          {
              if (Debug.isDebugBuild)
                    Debug.LogError("Invalid format at line " + linenumber +": '" + line + "'");
          }
          if (m_settings.ContainsKey(tokens[0])) 
          {
              //if (Debug.isDebugBuild) Debug.Log(tokens[0] + "=" + tokens[1]);
              m_settings[tokens[0]] = tokens[1];
          }
          else
          {
                Debug.LogWarning("Ignoring unknown parameter name '" + tokens[0] + "' at line "
                        + linenumber + ".");
      	}           
      }
		}
			
		reader.Close();  
	}
		
	/// <summary>
	/// Return current value of a given parameter.
	/// </summary>
	/// <param name='paramName'>
	/// Parameter name.
	/// </param>
	/// <param name='DEFAULT'>
	/// DEFAUL.
	/// </param>
	public string get(string paramName, string DEFAULT="")
	{
	    if (m_settings.ContainsKey(paramName)) {
	        return (string) m_settings[paramName];
	    } else {
	        return DEFAULT;
	    }
	}
	
	public long getLong(string paramName, long DEFAULT=0)
	{
	    if (m_settings.ContainsKey(paramName)) {
	        return long.Parse((string)m_settings[paramName]);
	    } else {
	        return DEFAULT;
	    }
	}
	
	public int getInt(string paramName, int DEFAULT=0)
	{
	    if (m_settings.ContainsKey(paramName)) {
	        return int.Parse((string)m_settings[paramName]);
	    } else {
	        return DEFAULT;
	    }
	}
	
	public float getFloat(string paramName, long DEFAULT=0)
	{
	    if (m_settings.ContainsKey(paramName)) {
	        return float.Parse((string)m_settings[paramName]);
	    } else {
	        return DEFAULT;
	    }
	}		

	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Private Member Functions

	//---------------------------------------------------------------------------
			
	
			
	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

	#region Other Members

	//---------------------------------------------------------------------------		
		
		
		
	//---------------------------------------------------------------------------

	#endregion

	//---------------------------------------------------------------------------

}// class OCConfig

}// namespace OpenCog



