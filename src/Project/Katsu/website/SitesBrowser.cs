using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Links.UrlBuilders;
using Sitecore.Resources;
using Sitecore.Resources.Media;
using Sitecore.Shell;
using Sitecore.Shell.Applications.Dialogs.MediaBrowser;
using Sitecore.Shell.Applications.Media.MediaBrowser;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XmlControls;
using System;
using System.Drawing;
using System.IO;
using System.Web.UI;

namespace Katsu.Project.Katsu
{
    public class SitesBrowser : DialogForm
    {
        /// <summary></summary>
        protected XmlControl Dialog;
        /// <summary>The media data context.</summary>
        protected DataContext MediaDataContext;
        /// <summary>The tree view of content items.</summary>
        protected TreeviewEx Treeview;
        /// <summary>The edit field for file name.</summary>
        protected Edit Filename;
        /// <summary>The scroll box for list view.</summary>
        protected Scrollbox Listview;
        /// <summary>The upload button</summary>
        protected Button UploadButton;

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull((object)e, nameof(e));
            //Telemetry.TelemetryClient.Track(Telemetry.Media.MediaBrowserActivated, 1UL);
            //Telemetry.TelemetryClient.Track(Telemetry.Media.MediaBrowserOpened, 1UL);
            //base.OnLoad(e);
            if (Sitecore.Context.ClientPage.IsEvent)
                return;
            MediaBrowserOptions mediaBrowserOptions = MediaBrowserOptions.Parse();
            Database masterDateBase = Factory.GetDatabase("master");
            Item root = masterDateBase.GetItem("/sitecore/media library/Project/Common/Packages");
            Item selectedItem =root;
            //Language language = root != null ? root.Language : selectedItem?.Language;
            var language = LanguageManager.DefaultLanguage;
            Assert.IsNotNull((object)language, "Language can't be determined.");
            this.MediaDataContext.Language = language;
            if (root != null)
                this.MediaDataContext.Root = root.ID.ToString();
            if (selectedItem != null)
                this.MediaDataContext.SetFolder(selectedItem.Uri);
            Item folder = this.MediaDataContext.GetFolder();
            Assert.IsNotNull((object)folder, "Item not found.");
            this.UpdateSelection(folder);
        }

        /// <summary>Updates the list view.</summary>
        /// <param name="item">The item.</param>
        private void UpdateSelection(Item item)
        {
            Assert.ArgumentNotNull((object)item, nameof(item));
            this.MediaDataContext.SetFolder(item.Uri);
            this.Treeview.SetSelectedItem(item);
            HtmlTextWriter output = new HtmlTextWriter((TextWriter)new StringWriter());

            SitesBrowser.RenderPreview(output, item);
            string str = output.InnerWriter.ToString();
            if (string.IsNullOrEmpty(str))
            {
                SitesBrowser.RenderEmpty(output);
                str = output.InnerWriter.ToString();
            }
            this.Listview.InnerHtml = str;
        }


        protected void SelectPackage()
        {
            Item selectionItem = this.Treeview.GetSelectionItem(this.MediaDataContext.Language, Sitecore.Data.Version.Latest);
            if (selectionItem == null)
                return;
            this.UpdateSelection(selectionItem);
        }

        /// <summary>Renders the list view item.</summary>
        /// <param name="output">The output.</param>
        /// <param name="item">The child.</param>
        private static void RenderListviewItem(HtmlTextWriter output, Item item)
        {
            Assert.ArgumentNotNull((object)output, nameof(output));
            Assert.ArgumentNotNull((object)item, nameof(item));
            MediaItem mediaItem = item;
            output.Write("<a href=\"#\" class=\"scTile\" onclick=\"javascript:return scForm.postEvent(this,event,'Listview_Click(&quot;" + (object)item.ID + "&quot;)')\">");
            output.Write("<div class=\"scTileImage\">");
            if (item.TemplateID == TemplateIDs.Folder || item.TemplateID == TemplateIDs.TemplateFolder || item.TemplateID == TemplateIDs.MediaFolder)
            {
                new ImageBuilder()
                {
                    Src = item.Appearance.Icon,
                    Width = 48,
                    Height = 48,
                    Margin = "24px 24px 24px 24px"
                }.Render(output);
            }
            else
            {
                MediaUrlBuilderOptions thumbnailOptions = MediaUrlBuilderOptions.GetThumbnailOptions((MediaItem)item);
                MediaItem thumbnail = thumbnailOptions.Database.GetItem(item.Appearance.Thumbnail);
                thumbnailOptions.UseDefaultIcon = true;
                thumbnailOptions.Width = 96;
                thumbnailOptions.Height = 96;
                thumbnailOptions.Language = item.Language;
                thumbnailOptions.AllowStretch = false;

                string mediaUrl = MediaManager.GetMediaUrl(thumbnail, thumbnailOptions);
                output.Write("<img src=\"" + mediaUrl + "\" class=\"scTileImageImage\" border=\"0\" alt=\"\" />");
            }
            output.Write("</div>");
            output.Write("<div class=\"scTileHeader\">");
            output.Write(item.GetUIDisplayName());
            output.Write("</div>");
            output.Write("</a>");
        }

        /// <summary>Renders the preview.</summary>
        /// <param name="output">The output.</param>
        /// <param name="item">The item.</param>
        private static void RenderPreview(HtmlTextWriter output, Item item)
        {
            Assert.ArgumentNotNull((object)output, nameof(output));
            Assert.ArgumentNotNull((object)item, nameof(item));
            MediaItem mediaItem = (MediaItem)item;
            string itemUrl = Sitecore.Resources.Media.MediaManager.GetMediaUrl(mediaItem);
            ImageField thumbnail =(ImageField) item.Fields["__Thumbnail"];
            output.Write("<table width=\"100%\" height=\"100%\" border=\"0\"><tr><td align=\"center\">");
            output.Write("<div class=\"scPreview\">");
            output.Write("<img src=\"" + Sitecore.Resources.Media.MediaManager.GetMediaUrl(thumbnail.MediaItem) + "\" class=\"scPreviewImage\" border=\"0\" alt=\"\" />");
            output.Write("</div>");
            output.Write("<div class=\"scPreviewHeader\">");
            output.Write(item.GetUIDisplayName());
            output.Write("</div>");
            output.Write("</td></tr></table>");
        }

        /// <summary>Renders the empty.</summary>
        /// <param name="output">The output.</param>
        private static void RenderEmpty(HtmlTextWriter output)
        {
            Assert.ArgumentNotNull((object)output, nameof(output));
            output.Write("<table width=\"100%\" border=\"0\"><tr><td align=\"center\">");
            output.Write("<div style=\"padding:8px\">");
            output.Write(Translate.Text("This folder is empty."));
            output.Write("</div>");
            output.Write("<div class=\"scUploadLink\" style=\"padding:8px\">");
            new Tag("a")
            {
                Href = "#",
                Click = "scForm.postRequest('', '', '', 'UploadImage');",
                InnerHtml = (Translate.Text("Upload a File.") + ".")
            }.ToString(output);
            output.Write("</div>");
            output.Write("</td></tr></table>");
        }

        public void CancelPackage()
        {
            SheerResponse.CloseWindow();


        }

        public void SubmitPackage()
        {

            MediaBrowserOptions mediaBrowserOptions = MediaBrowserOptions.Parse();
            var str = Treeview.GetSelectionItem();
            if (str == null)
            {
                SheerResponse.Alert(Translate.Text("Select a package."));
            }
            else
            {

                    SheerResponse.SetDialogValue(str.ID.ToString());
                    SheerResponse.CloseWindow();
                
            }

        }
        private static bool IsFolderItem(Item item)
        {
            Assert.ArgumentNotNull((object)item, nameof(item));
            return item.TemplateID == TemplateIDs.Node || item.TemplateID == TemplateIDs.Folder || item.TemplateID == TemplateIDs.MediaFolder;
        }
    }

}
