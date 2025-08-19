using System;
using UnityEngine;

namespace binc.PixelAnimator.DataManipulations
{
	[System.Serializable]
	public class SerializableType
	{
		public override int GetHashCode()
		{
			return HashCode.Combine(name, assemblyQualifiedName, assemblyName, systemType);
		}
		public string Name => name;
		[SerializeField] private string name;

		public string AssemblyQualifiedName => assemblyQualifiedName;
		[SerializeField] private string assemblyQualifiedName;
	
		public string AssemblyName => assemblyName;
		[SerializeField] private string assemblyName;
	
		public Type SystemType
		{
			get 	
			{
				if (systemType == null)	
				{
					GetSystemType();
				}
				return systemType;
			}
		}
		private Type systemType;	
	
		private void GetSystemType(){
			systemType = Type.GetType(assemblyQualifiedName);
		}
	
		public SerializableType( Type systemType ){
			if (systemType == null) return;
			this.systemType = systemType;
			name = systemType.Name;
			assemblyQualifiedName = systemType.AssemblyQualifiedName;
			assemblyName = systemType.Assembly.FullName;
		}
	
		public override bool Equals( System.Object obj ){
			SerializableType temp = obj as SerializableType;
			if ((object)temp == null)
			{
				return false;
			}
			return Equals(temp);
		}
	
		public bool Equals( SerializableType @object){
			return @object.SystemType == SystemType;
		}
	
		public static bool operator ==( SerializableType a, SerializableType b ){
			if (System.Object.ReferenceEquals(a, b))
			{
				return true;
			}
	
			if (((object)a == null) || ((object)b == null))
			{
				return false;
			}
			return a.Equals(b);
		}
	
		public static bool operator !=( SerializableType a, SerializableType b ){
			return !(a == b);
		}
	}
}
