// Simple helper class that allows you to serialize System.Type objects.
// Use it however you like, but crediting or even just contacting the author would be appreciated (Always 
// nice to see people using your stuff!)
//
// Written by Bryan Keiren (http://www.bryankeiren.com)

using System;
using UnityEngine;

[Serializable]
public class SerializableType
{
	[SerializeField] private string mName;
	
	public string Name => mName;

	[SerializeField] private string mAssemblyQualifiedName;
	
	public string AssemblyQualifiedName => mAssemblyQualifiedName;

	[SerializeField] private string mAssemblyName;
	
	public string AssemblyName => mAssemblyName;

	private Type mSystemType;	
	public Type SystemType
	{
		get 	
		{
			if (mSystemType == null)	
			{
				GetSystemType();
			}
			return mSystemType;
		}
	}
	
	private void GetSystemType()
	{
		mSystemType = Type.GetType(mAssemblyQualifiedName);
	}
	
	public SerializableType( Type systemType )
	{
		if (systemType == null) return;
		mSystemType = systemType;
		mName = systemType.Name;
		mAssemblyQualifiedName = systemType.AssemblyQualifiedName;
		mAssemblyName = systemType.Assembly.FullName;

	}
	
	public override bool Equals( System.Object obj )
	{
		var temp = obj as SerializableType;
		return (object)temp != null && Equals(temp);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(mName, mAssemblyQualifiedName, mAssemblyName, mSystemType);
	}

	public bool Equals( SerializableType @object )
	{
		return @object.SystemType == SystemType;
	}
	
	public static bool operator ==( SerializableType a, SerializableType b )
	{
	    if (ReferenceEquals(a, b))
	    {
	        return true;
	    }
	
	    if ((object)a == null || (object)b == null)
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
