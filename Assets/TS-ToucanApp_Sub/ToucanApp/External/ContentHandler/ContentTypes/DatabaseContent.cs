using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.Data
{
    public class DatabaseContent : AbstractContent<TableData> 
    {
//        public enum FieldType
//        {
//            boolData,
//            intData,
//            doubleData,
//            textData,
//            imageData,
//            videoData,
//            audioData,
//            photoGaleryData,
//            videoGaleryData,
//            auidoGaleryData,
//            mapData,
//        }
//
//        [System.Serializable]
//        public class FieldsData
//        {
//            public string name;
//            public FieldType type;
//        }
//
//        public FieldsData[] fields;
//
//        public override void OnExportData()
//        {
//            base.OnExportData();
//
//            Data.AddChild<RowData>("row", (RowData row) => {
//
//                foreach (var field in fields)
//                {
//                    switch (field.type)
//                    {
//                        case FieldType.boolData:
//                            row.AddChild<BoolData>(field.name);
//                            break;
//                        case FieldType.intData:
//                            row.AddChild<IntData>(field.name);
//                            break;
//                        case FieldType.doubleData:
//                            row.AddChild<DoubleData>(field.name);
//                            break;
//                        case FieldType.textData:
//                            row.AddChild<TextData>(field.name);
//                            break;
//                        case FieldType.imageData:
//                            row.AddChild<ImageData>(field.name);
//                            break;
//                        case FieldType.videoData:
//                            row.AddChild<VideoData>(field.name);
//                            break;
//                        case FieldType.audioData:
//                            row.AddChild<AudioData>(field.name);
//                            break;
//                        case FieldType.photoGaleryData:
//                            row.AddChild<GaleryData>(field.name, (GaleryData galery) => galery.AddChild<ImageData>("item"));
//                            break;
//                        case FieldType.videoGaleryData:
//                            row.AddChild<GaleryData>(field.name, (GaleryData galery) => galery.AddChild<VideoData>("item"));
//                            break;
//                        case FieldType.auidoGaleryData:
//                            row.AddChild<GaleryData>(field.name, (GaleryData galery) => galery.AddChild<AudioData>("item"));
//                            break;
//                        case FieldType.mapData:
//                            row.AddChild<MapData>(field.name);
//                            break;
//                    }
//                }
//
//            });
//        }
    }
}
