using UnityEngine;
using System;
using System.Collections.Generic;


namespace binc.PixelAnimator.DataProvider{
    
    

    [Serializable]
    public class BasicPropertyData{
        [SerializeField] private string name;
        public string Name => name;
        public DataType dataType;
        [ReadOnly, SerializeField] private string guid;

        public string Guid => guid;

        public BasicPropertyData(string name, string guid){
            this.name = name;
            this.guid = guid;
        }
        
    }
    
    [Serializable]
    public class PropertyData{
        public List<GenericData> genericData;
        public List<string> eventNames;

        public PropertyData(){
            genericData = new List<GenericData>();
            eventNames = new List<string>();
        }
    }
    
    

}