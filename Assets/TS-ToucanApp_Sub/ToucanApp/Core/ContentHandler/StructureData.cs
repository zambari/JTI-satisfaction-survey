using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace ToucanApp.Data
{
    public interface IRootDataChild {}
    public interface IHierarchyDataChild {}
    public interface ITableDataChild {}
    public interface IRowDataChild {}
    public interface IGaleryDataChild {}
    public interface IArticleDataChild {}

    public class DataBuilder
    {
        private Dictionary<string, BaseData> storedData = new Dictionary<string, BaseData>();

        public static DataBuilder FromTree(BaseData root)
        {
            var builder = new DataBuilder();

            BaseData.ApplyAction(root, (BaseData next) => {

                if (next != null && !string.IsNullOrEmpty(next.id) )
                {
                    var json = LitJson.JsonMapper.ToJson(next);
                    var clone = FromJson(json, next.GetType());

                    if (!builder.storedData.ContainsKey(clone.id))
                        builder.storedData.Add(clone.id, clone);
                }

            });

            builder.ToTree();

            return builder;
        }

        public static DataBuilder FromJson(string json)
        {
            var builder = new DataBuilder();

            var deserializedArray = LitJson.JsonMapper.ToObject(json);
            int itemsCount = deserializedArray.Count;

            for (int i = 0; i < itemsCount; i++)
            {
                var deserializedItem = deserializedArray[i];    
                var itemJson = deserializedItem.ToJson();
                var itemType = System.Type.GetType((string)deserializedItem["type"]);
                var data = FromJson(itemJson, itemType);

                builder.storedData.Add(data.id, data);
            }

            builder.ToTree();

            return builder;
        }

        public BaseData FindData(string id)
        {
            BaseData found;
            storedData.TryGetValue(id, out found);
            return found;
        }

        public BaseData FindOriginalData(string id)
        {
            return storedData.Values.FirstOrDefault(item => item.originalID == id); // slow replace with separate dictionary
        }

        public BaseData ToTree()
        {
            var root = storedData.FirstOrDefault().Value;
            if (root != null)
                BuildTree(root);

            return root;
        }

        public BaseData[] ToArray()
        {
            return storedData.Values.ToArray();
        }

        private void BuildTree(BaseData parent)
        {
            var temp = new List<BaseData>();
            foreach (var data in storedData.Values)
            {
                if (data.parentID == parent.id)
                {
                    temp.Add(data);

                    BuildTree(data);
                }
            }

            parent.SetChildren(temp.ToArray());
        }
            
        public static BaseData FromJson(string json, System.Type type)
        {
            BaseData obj = null;

            if (type == typeof(BaseData))
            {
                obj = LitJson.JsonMapper.ToObject<BaseData>(json);
            }
            else if (type == typeof(RootData))
            {
                obj = LitJson.JsonMapper.ToObject<RootData>(json);
            }
            else if (type == typeof(HierarchyData))
            {
                obj = LitJson.JsonMapper.ToObject<HierarchyData>(json);
            }
            else if (type == typeof(TableData))
            {
                obj = LitJson.JsonMapper.ToObject<TableData>(json);
            }
            else if (type == typeof(RowData))
            {
                obj = LitJson.JsonMapper.ToObject<RowData>(json);
            }
            else if (type == typeof(ArticleData))
            {
                obj = LitJson.JsonMapper.ToObject<ArticleData>(json);
            } 
            else if (type == typeof(GaleryData))
            {
                obj = LitJson.JsonMapper.ToObject<GaleryData>(json);
            }
            else if (type == typeof(AudioData))
            {
                obj = LitJson.JsonMapper.ToObject<AudioData>(json);
            } 
            else if (type == typeof(ImageData))
            {
                obj = LitJson.JsonMapper.ToObject<ImageData>(json);
            }
            else if (type == typeof(MapData))
            {
                obj = LitJson.JsonMapper.ToObject<MapData>(json);
            }
            else if (type == typeof(VideoData))
            {
                obj = LitJson.JsonMapper.ToObject<VideoData>(json);
            }
            else if (type == typeof(TextData))
            {
                obj = LitJson.JsonMapper.ToObject<TextData>(json);
            }
            else if (type == typeof(BoolData))
            {
                obj = LitJson.JsonMapper.ToObject<BoolData>(json);
            }
            else if (type == typeof(DoubleData))
            {
                obj = LitJson.JsonMapper.ToObject<DoubleData>(json);
            }
            else if (type == typeof(IntData))
            {
                obj = LitJson.JsonMapper.ToObject<IntData>(json);
            }
            else if (type == typeof(EnumData))
            {
                obj = LitJson.JsonMapper.ToObject<EnumData>(json);
            }
            else if (type == typeof(ColorData))
            {
                obj = LitJson.JsonMapper.ToObject<ColorData>(json);
            }
            else if (type == typeof(TextareaData))
            {
                obj = LitJson.JsonMapper.ToObject<TextareaData>(json);
            }
            else if (type == typeof(StateLinkData))
            {
                obj = LitJson.JsonMapper.ToObject<StateLinkData>(json);
            }
            else if (type == typeof(TableLinkData))
            {
                obj = LitJson.JsonMapper.ToObject<TableLinkData>(json);
            }
            else if (type == typeof(PositionData))
            {
                obj = LitJson.JsonMapper.ToObject<PositionData>(json);
            }
            else if (type == typeof(AssetData))
            {
                obj = LitJson.JsonMapper.ToObject<AssetData>(json);
            }
            else if (type == typeof(SubtitleData))
            {
                obj = LitJson.JsonMapper.ToObject<SubtitleData>(json);
            }
            else
            {
                if (type != null)
                {
                    Debug.LogError((string)LitJson.JsonMapper.ToObject(json)["type"] + " can't cast to type -> " + type.ToString());
                }
                else
                {
                    Debug.LogError((string)LitJson.JsonMapper.ToObject(json)["objectName"] + " type is null!");
                }
            }

            return obj;
        }

        public BaseData[] CheckStructure()
        {
            List<BaseData> temp = new List<BaseData>();
            foreach (var data in storedData.Values)
            {
                var parent = FindData(data.parentID);
                if (parent != null)
                {
                    var hierarchyData = data as HierarchyData;
                    if (hierarchyData != null)
                    {
                        if (hierarchyData.isPrototype && parent as RootData != null)
                            temp.Add(hierarchyData);
                    }

                    if ((data as IRootDataChild != null && parent as RootData != null) ||
                        (data as IHierarchyDataChild != null && parent as HierarchyData != null) ||
                        (data as ITableDataChild != null && parent as TableData != null) ||
                        (data as IRowDataChild != null && parent as RowData != null) ||
                        (data as IGaleryDataChild != null && parent as GaleryData != null) ||
                        (data as IArticleDataChild != null && parent as ArticleData != null))
                            continue;

                    temp.Add(data);
                }
            }

            return temp.ToArray();
        }

        public static bool Compare(DataBuilder b1, DataBuilder b2)
        {
            if (b1 == b2)
                return true;

            foreach (var candidate in b1.storedData.Values)
            {
                if (candidate.isClone)
                    continue;

                var found = b2.FindData(candidate.id);
                if (found == null || found.type != candidate.type)
                    return false;
            }

            return true;
        }
    }

	public class BaseData
	{
        public string type;
        public string id;
        public string originalID;
        public string parentID;
        public string tag;
		public string objectName;
        public bool isActive = true;
		public bool isClone = false;
        public string alias;

        private bool markedForStorage = false;
        private BaseData[] children = new BaseData[0];

        public void SetChildren(BaseData[] children)
        {
            this.children = children;
        }

        public void MarkForStorage()
        {
            markedForStorage = true;
        }

        public string Local(string var_name)
        {
            return id + var_name;
        }

        public bool IsMarkedForStorage()
        {
            return markedForStorage;
        }

        private BaseData DummyData()
        {
            var builder = DataBuilder.FromTree(this);
            var copy = builder.ToTree();
            return copy;
        }

        public BaseData CloneData()
        {
            var builder = DataBuilder.FromTree(this);
            var copy = builder.ToTree();

            ApplyAction(copy, (BaseData data) => { data.id = GetRandomID(); });

            return copy;
        }

        public T GetFirstChild<T>(string tag) where T : BaseData, new()
        {
            return GetFirstChild((T data) => (data.tag == tag));
        }

        public T GetFirstChild<T>(Func<T, bool> match = null) where T : BaseData, new()
        {
            if (children != null)
            {
                foreach (var child in children)
                {
                    var cast = child as T;
                    if (cast != null)
                    {
                        if (match == null || match(cast))
                        {
                            // mark for storage only data added from script.
                            if (!string.IsNullOrEmpty(child.tag))
                                cast.MarkForStorage();

                            return cast;
                        }
                    }
                }
            }

            return new T();
        }

        public T[] PeekChildren<T>() where T : BaseData
        {
            return GetChildren<T>(null, false);
        }

        public T[] GetChildren<T>(string tag) where T : BaseData
        {
            return GetChildren((T data) => (data.tag == tag), true);
        }

        public T[] GetChildren<T>(Func<T, bool> match = null, bool used = true) where T : BaseData
        {
            List<T> temp = new List<T>();

            if (children != null)
            {
                foreach (var child in children)
                {
                    var cast = child as T;
                    if (cast != null)
                    {
                        if (match == null || match(cast))
                        {
                            // mark for storage only data added from script.
                            if (used && !string.IsNullOrEmpty(child.tag))
                                child.MarkForStorage();

                            temp.Add(cast);
                        }
                    }
                }
            }

            return temp.ToArray();
        }

        public void AddChild<T>(string tag, Action<T> action = null, bool insert = false) where T : BaseData, new()
        {
            bool exists = false;

            if (children != null)
            {
                foreach (var child in children)
                {
                    if (child.tag == tag)
                    {
                        var found = child as T;
                        if (found != null)
                        {
                            if (action != null)
                            {
                                T fake = found.DummyData() as T;
                                fake.objectName += " (DUMMY)";
                                action(fake);

                                found.children = fake.children;
                            }

                            exists = true;
                        }
                    }
                }
            }

            if (!exists)
            {
                var newData = AddChild<T>(tag, insert);

                if (action != null)
                    action(newData);
            }
        }

        private T AddChild<T>(string tag, bool insert = false) where T : BaseData, new()
        {
            List<BaseData> temp = new List<BaseData>();
            if (children != null)
                temp.AddRange(children);

            var id = GetRandomID();

            var found = new T();
            found.id = id;
            found.originalID = id;
            found.tag = tag;
            found.objectName = tag;
            found.parentID = this.id;
            found.type = typeof(T).ToString();

            if (insert)
                temp.Insert(0, found);
            else
                temp.Add(found);

            children = temp.ToArray();

            return found;
        }

        public void ClearUnusedData()
        {
            if (children != null)
            {
                List<BaseData> temp = new List<BaseData>();

                foreach (var child in children)
                {
                    if (child.IsMarkedForStorage())
                    {
                        temp.Add(child);
                    }

                    child.ClearUnusedData();
                }

                children = temp.ToArray ();
            }
        }

        public static void ApplyAction(BaseData candidate, Action<BaseData> action)
        {
            action(candidate);

            foreach (var child in candidate.children)
            {
                ApplyAction(child, action);
            }
        }

        public static T FindData<T>(BaseData candidate, Func<BaseData, bool> match) where T : BaseData
        {
            if (match(candidate))
                return candidate as T;

            foreach (var child in candidate.children)
            {
                var childCanditate = FindData<T>(child, match);
                if (childCanditate != null)
                    return childCanditate;
            }

            return null;
        }

        public static string GetRandomID()
        {
            return System.IO.Path.GetFileNameWithoutExtension(System.IO.Path.GetRandomFileName());
        }
	}

    public class RootData : BaseData
    {
        public string AppID;
        public string buildVersion;
        public string contentVersion;
        public string[] modules;
        public string[] translations = new string[1];
    }

    public class HierarchyData : BaseData, IRootDataChild, IHierarchyDataChild
    {
        public bool linkable;
        public bool isPrototype;
    }

    public class ArticleData : BaseData, IHierarchyDataChild, IRowDataChild
    {

    }

    public class TableData : BaseData, IHierarchyDataChild
    {

    }

    public class RowData : BaseData, ITableDataChild
	{

	}

    public class GaleryData : BaseData, IHierarchyDataChild, IRowDataChild, IArticleDataChild
    {

	}

    public class TranslatedData : BaseData
    {
        public string[] translations = new string[1];

        public int GetTranslationIdx(int id)
        {
            if (translations != null && translations.Length > 0)
            {
                if (id >= 0 && id < translations.Length && !string.IsNullOrEmpty(translations[id]))
                {
                    return id;
                }

                return  0;
            }

            return -1;
        }

        public string GetTranslation(int id)
        {
            int fid = GetTranslationIdx(id);

            if (fid > -1)
                return translations[fid];

            return null;
        }
    }

    public class ResourceData : TranslatedData
    {
        public string[] hash = new string[1];
    }

    public class TextData : TranslatedData, IHierarchyDataChild, IRowDataChild, IArticleDataChild
    {

    }

    public class TextareaData : TextData, IHierarchyDataChild, IRowDataChild, IArticleDataChild
    {
        public int lines = -1;
    }

    public class ImageData : ResourceData, IHierarchyDataChild, IRowDataChild, IGaleryDataChild, IArticleDataChild
    {
        public int resolutionX;
        public int resolutionY;
    }

    public class AudioData : ResourceData, IHierarchyDataChild, IRowDataChild, IGaleryDataChild, IArticleDataChild
    {
        public double volume = 100;
        public double pitch = 1;
    }

    public class VideoData : ResourceData, IHierarchyDataChild, IRowDataChild, IGaleryDataChild, IArticleDataChild
    {
        public double volume = 100;
    }

    public class SubtitleData : ResourceData, IHierarchyDataChild, IRowDataChild, IGaleryDataChild, IArticleDataChild
    {

    }

    public class AssetData : ResourceData, IHierarchyDataChild, IRowDataChild, IGaleryDataChild, IArticleDataChild
    {
        public string filename;
    }

    public class IntData : BaseData, IHierarchyDataChild, IRowDataChild
    {
        public int value;
    }

    public class EnumData : IntData, IHierarchyDataChild, IRowDataChild, IArticleDataChild
    {
        public string[] options;
    }

    public class DoubleData : BaseData, IHierarchyDataChild, IRowDataChild
    {
        public double value;
    }

    public class BoolData : BaseData, IHierarchyDataChild, IRowDataChild
    {
        public bool value;
    }

    public class ColorData : BaseData, IHierarchyDataChild, IRowDataChild
    {
        public string color;

        public ColorData()
        {
            SetColor(Color.black);
        }

        public Color GetColor(Color defalutColor)
        {
            Color colorCache = defalutColor;

            var colorString = color;

            if (string.IsNullOrEmpty(colorString))
                return colorCache;

            if (colorString.IndexOf('#') > -1)
            {
                Color newCol;
                if (ColorUtility.TryParseHtmlString(colorString, out newCol))
                    colorCache = newCol;
            }
            else
            {
                var start = colorString.IndexOf('(') + 1;
                var end = colorString.IndexOf(')');
                colorString = colorString.Substring(start, end - start);
                var rgbaStrings = colorString.Split(',');
                var colorArray = new float[rgbaStrings.Length];

                for (int i = 0; i < rgbaStrings.Length; i++)
                {
                    colorArray[i] = System.Convert.ToSingle(rgbaStrings[i]);

                    if (colorArray[i] > 1)
                        colorArray[i] /= 255;
                }

                if (colorArray.Length == 3)
                {
                    colorCache = new Color(colorArray[0], colorArray[1], colorArray[2]);
                }
                else
                {
                    colorCache = new Color(colorArray[0], colorArray[1], colorArray[2], colorArray[3]);
                }
            }

            return colorCache; 
        }

        public void SetColor(Color c)
        {
            //color = string.Format("rgba({0}, {1}, {2}, {3})", (int)(c.r * 255), (int)(c.g * 255), (int)(c.b * 255), c.a);
            color = "#" + ColorUtility.ToHtmlStringRGB(c);
            //colorCache = color;
        }
    }

    public class PositionData : BaseData, IHierarchyDataChild, IRowDataChild
    {
        public double x;
        public double y;

        public Vector2 Vector()
        {
            return new Vector2((float)x, (float)y);
        }
    }

    public class MapData : BaseData, IHierarchyDataChild, IRowDataChild
    {
        public double lat;
        public double log;
    }

    public class StateLinkData : BaseData, IHierarchyDataChild, IRowDataChild
    {
        public string stateID;
    }

    public class TableLinkData : BaseData, IHierarchyDataChild, IRowDataChild
    {
        public string linkedTable;
        public string[] linkedRows = new string[0];
    }
}

