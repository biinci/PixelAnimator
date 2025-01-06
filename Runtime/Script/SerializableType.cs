// Simple helper class that allows you to serialize System.Type objects.
// Use it however you like, but crediting or even just contacting the author would be appreciated (Always 
// nice to see people using your stuff!)
//
// Written by Bryan Keiren (http://www.bryankeiren.com)

using UnityEngine;
using System.Runtime.Serialization;

[System.Serializable]
public class SerializableType
{
	[SerializeField]
	private string m_Name;
	
	public string Name
	{
		get { return m_Name; }
	}
	
	[SerializeField]
	private string m_AssemblyQualifiedName;
	
	public string AssemblyQualifiedName
	{
		get { return m_AssemblyQualifiedName; }
	}
	
	[SerializeField]
	private string m_AssemblyName;
	
	public string AssemblyName
	{
		get { return m_AssemblyName; }
	}
	
	private System.Type m_SystemType;	
	public System.Type SystemType
	{
		get 	
		{
			if (m_SystemType == null)	
			{
				GetSystemType();
			}
			return m_SystemType;
		}
	}
	
	
	private void GetSystemType()
	{
		m_SystemType = System.Type.GetType(m_AssemblyQualifiedName);
	}
	
	public SerializableType( System.Type systemType )
	{
		if (systemType == null) return;
		m_SystemType = systemType;
		m_Name = systemType.Name;
		m_AssemblyQualifiedName = systemType.AssemblyQualifiedName;
		m_AssemblyName = systemType.Assembly.FullName;

	}
	
	public override bool Equals( System.Object obj )
	{
		SerializableType temp = obj as SerializableType;
		if ((object)temp == null)
		{
			return false;
		}
		return this.Equals(temp);
	}
	
	public bool Equals( SerializableType _Object )
	{
		//return m_AssemblyQualifiedName.Equals(_Object.m_AssemblyQualifiedName);
		return _Object.SystemType.Equals(SystemType);
	}
	
	public static bool operator ==( SerializableType a, SerializableType b )
	{
	    // If both are null, or both are same instance, return true.
	    if (System.Object.ReferenceEquals(a, b))
	    {
	        return true;
	    }
	
	    // If one is null, but not both, return false.
	    if (((object)a == null) || ((object)b == null))
	    {
	        return false;
	    }
	
	    return a.Equals(b);
	}
	
	public static bool operator !=( SerializableType a, SerializableType b )
	{
	    return !(a == b);
	}
}
