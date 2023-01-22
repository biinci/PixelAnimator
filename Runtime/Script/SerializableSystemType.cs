using System;
using UnityEngine;

namespace binc.PixelAnimator{
	[Serializable]
	public class SerializableSystemType{
	
		[SerializeField] private string name;
	
		public string Name => name;


		[SerializeField] private string assemblyQualifiedName;
	
		public string AssemblyQualifiedName => assemblyQualifiedName;

	
		[SerializeField] private string mAssemblyName;
	
		public string AssemblyName => mAssemblyName;

		private Type systemType;	
		public Type SystemType{
			get 	
			{
				if (systemType == null){
					GetSystemType();
				}
				return systemType;
			}
		}
	
		private void GetSystemType(){
			systemType = Type.GetType(assemblyQualifiedName);
		}
	
		public SerializableSystemType( Type systemType ){ 
			this.systemType = systemType;
			name = systemType.Name;
			assemblyQualifiedName = systemType.AssemblyQualifiedName;
			mAssemblyName = systemType.Assembly.FullName;
		}

	
	
		public override bool Equals( object obj ){ 
			var temp = obj as SerializableSystemType;
			return (object)temp != null && this.Equals(temp);
		}
	
		public bool Equals( SerializableSystemType @object ){ 
			return @object.SystemType == SystemType;
		}
	
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public static bool operator ==( SerializableSystemType a, SerializableSystemType b ){
			// If both are null, or both are same instance, return true.
			if (ReferenceEquals(a, b)){
				return true;
			}
	
			// If one is null, but not both, return false.
			if ( ( (object) a == null ) || ( (object) b == null ) ){
				return false;
			}
	
			return a.Equals(b);
		}
	
		public static bool operator !=( SerializableSystemType a, SerializableSystemType b ){
			return !(a == b);
		}


	}
}