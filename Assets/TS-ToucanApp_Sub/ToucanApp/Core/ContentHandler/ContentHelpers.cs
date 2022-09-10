using System.Collections.Generic;
using UnityEngine;

namespace ToucanApp.Data
{
    public enum ArticleImageType
    {
        None,
        Texture,
        Sprite,
    }

    public class ArticleInfo
    {
        public BaseData data;
        public ResourceInfo[] resources;
    }

    public class ContentHelpers : MonoBehaviour
    {
        public ContentHandler ContentHandler
        {
            get;
            private set;
        }

        private void Awake()
        {
            ContentHandler = GetComponentInParent<ContentHandler>();
        }

        public string GetText(BaseData parentData, string tag)
        {
            if (parentData == null)
                return null;

            var childData = parentData.GetFirstChild<TextData>(tag);
            if (childData != null)
            {
                return childData.GetTranslation(ContentHandler.LanguageID);
            }

            return null;
        }

        public string[] GetTexts(BaseData parentData, string tag)
        {
            if (parentData == null)
                return null;

            var childrenData = parentData.GetChildren<TextData>(tag);
            if (childrenData != null)
            {
                string[] temp = new string[childrenData.Length];
                for (int i = 0; i < childrenData.Length; i++)
                {
                    temp[i] = childrenData[i].GetTranslation(ContentHandler.LanguageID);
                }

                return temp;
            }

            return null;
        }

        public void GetResource(BaseData parentData, string tag, System.Action<ResourceInfo> onDone)
        {
            if (parentData == null)
                return;

            var childData = parentData.GetFirstChild<ResourceData>(tag);
            if (childData != null)
            {
                ContentHandler.Resources.Load(childData, (ResourceInfo info) => {

                    onDone.Invoke(info);

                });
            }
        }

        public void GetResources(BaseData parentData, string tag, System.Action<ResourceInfo, int> onDone)
        {
            if (parentData == null)
                return;

            var childrenData = parentData.GetChildren<ResourceData>(tag);
            if (childrenData != null)
            {
                for (int i = 0; i < childrenData.Length; i++)
                {
                    int idx = i;
                    ContentHandler.Resources.Load(childrenData[i], (ResourceInfo info) => {

                        onDone(info, idx);

                    });
                }
            }
        }

        public T[] GetTableData<T>(TableData parent, string tag, string table_tag = "table") where T : BaseData
        {
            List<T> temp = new List<T>();

            var table = parent.GetFirstChild<TableData>(table_tag);
            var rows = table.GetChildren<RowData>("row");

            foreach (var r in rows)
            {
                var found = r.GetChildren<T>(tag);
                temp.AddRange(found);
            }

            return temp.ToArray();
        }

        public void AddTableData(BaseData parent, System.Action<RowData> onDone, string table_tag = "table")
        {
            parent.AddChild<TableData>(table_tag, (TableData table) => {

                table.AddChild<RowData>("row", (RowData row) => {

                    onDone(row);

                });

            });
        }

        public T[] GetHierarchyData<T>(HierarchyData parent, string tag, string state_tag = "state") where T : BaseData
        {
            List<T> temp = new List<T>();

            var hierarchy = parent.GetFirstChild<HierarchyData>(state_tag);
            var children = hierarchy.GetChildren<HierarchyData>("child");

            foreach (var c in children)
            {
                var found = c.GetChildren<T>(tag);
                temp.AddRange(found);
            }

            return temp.ToArray();
        }

        public void AddHierarchyData<T>(HierarchyData parent, System.Action<HierarchyData> onDone, string state_tag = "state") where T : BaseData
        {
            parent.AddChild<HierarchyData>(state_tag, (HierarchyData owner) => {

                owner.AddChild<HierarchyData>("child", (HierarchyData child) => {

                    onDone(child);

                });

            });
        }

        public ArticleInfo[] GetArticleInfo(ArticleData parentData, ArticleImageType imageType = ArticleImageType.None)
        {
            List<ArticleInfo> temp = new List<ArticleInfo>();

            var childrenData = parentData.PeekChildren<BaseData>();
            if (childrenData != null)
            {
                var languagesCount = ContentHandler.RootContent.LanguagesCount;

                for (int i = 0; i < childrenData.Length; i++)
                {
                    var childData = childrenData[i];

                    if (!childData.isClone)
                        continue;

                    childData.MarkForStorage();

                    var articleInfo = new ArticleInfo();
                    articleInfo.data = childData;

                    var resourceData = (childData as ResourceData);
                    if (resourceData != null)
                    {
                        articleInfo.resources = new ResourceInfo[languagesCount];

                        for (int j = 0; j < languagesCount; j++)
                        {
                            int languageID = j;
                            ContentHandler.Resources.Load(resourceData, (ResourceInfo resource) =>
                            {
                                articleInfo.resources[languageID] = resource;
                                if (imageType == ArticleImageType.Texture)
                                {
                                    resource.GetTexture();
                                }
                                else if (imageType == ArticleImageType.Sprite)
                                {
                                    resource.GetSprite();
                                }

                            });
                        }
                    }

                    temp.Add(articleInfo);
                }
            }

            return temp.ToArray();
        }
    }
}
