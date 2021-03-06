using Sitecore;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Links.UrlBuilders;
using Sitecore.Resources;
using Sitecore.Resources.Media;
using Sitecore.Shell;
using Sitecore.Shell.Applications.Dialogs.MediaBrowser;
using Sitecore.Shell.Applications.Media.MediaBrowser;
using Sitecore.Web.UI.HtmlControls;
using System;
using System.Drawing;
using System.IO;
using System.Web.UI;

namespace Katsu.Project.Katsu
{
    public class SitesBrowser : MediaBrowserForm
    {
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull((object)e, nameof(e));
            //Telemetry.TelemetryClient.Track(Telemetry.Media.MediaBrowserActivated, 1UL);
            //Telemetry.TelemetryClient.Track(Telemetry.Media.MediaBrowserOpened, 1UL);
            //base.OnLoad(e);
            if (Sitecore.Context.ClientPage.IsEvent)
                return;
            MediaBrowserOptions mediaBrowserOptions = MediaBrowserOptions.Parse();
            Item root = mediaBrowserOptions.Root;
            Item selectedItem = mediaBrowserOptions.SelectedItem;
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
            this.Filename.Value = this.ShortenPath(item.Paths.Path);
            this.MediaDataContext.SetFolder(item.Uri);
            this.Treeview.SetSelectedItem(item);
            HtmlTextWriter output = new HtmlTextWriter((TextWriter)new StringWriter());
            if (item.TemplateID == TemplateIDs.Folder || item.TemplateID == TemplateIDs.MediaFolder || item.TemplateID == TemplateIDs.MainSection)
            {
                foreach (Item child in item.Children)
                {
                    if (child.Appearance.Hidden)
                    {
                        if (Sitecore.Context.User.IsAdministrator && UserOptions.View.ShowHiddenItems)
                            SitesBrowser.RenderListviewItem(output, child);
                    }
                    else
                        SitesBrowser.RenderListviewItem(output, child);
                }
            }
            else
                SitesBrowser.RenderPreview(output, item);
            string str = output.InnerWriter.ToString();
            if (string.IsNullOrEmpty(str))
            {
                SitesBrowser.RenderEmpty(output);
                str = output.InnerWriter.ToString();
            }
            this.Listview.InnerHtml = str;
            this.UploadButton.Disabled = !item.Access.CanCreate();
        }

        /// <summary>Renders the list view item.</summary>
        /// <param name="output">The output.</param>
        /// <param name="item">The child.</param>
        private static void RenderListviewItem(HtmlTextWriter output, Item item)
        {
            Assert.ArgumentNotNull((object)output, nameof(output));
            Assert.ArgumentNotNull((object)item, nameof(item));
            MediaItem mediaItem = (MediaItem)item;
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
                thumbnailOptions.UseDefaultIcon = new bool?(true);
                thumbnailOptions.Width = new int?(96);
                thumbnailOptions.Height = new int?(96);
                thumbnailOptions.Language = item.Language;
                thumbnailOptions.AllowStretch = new bool?(false);
                string mediaUrl = MediaManager.GetMediaUrl(mediaItem, thumbnailOptions);
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
            MediaUrlBuilderOptions shellOptions = MediaUrlBuilderOptions.GetShellOptions();
            shellOptions.AllowStretch = new bool?(false);
            shellOptions.BackgroundColor = new Color?(Color.White);
            shellOptions.Language = item.Language;
            shellOptions.UseDefaultIcon = new bool?(true);
            shellOptions.Height = new int?(192);
            shellOptions.Width = new int?(192);
            shellOptions.DisableBrowserCache = new bool?(true);
            string mediaUrl = MediaManager.GetMediaUrl(mediaItem, shellOptions);
            output.Write("<table width=\"100%\" height=\"100%\" border=\"0\"><tr><td align=\"center\">");
            output.Write("<div class=\"scPreview\">");
            output.Write("<img src=\"" + mediaUrl + "\" class=\"scPreviewImage\" border=\"0\" alt=\"\" />");
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

        /// <summary>Shortens the path.</summary>
        /// <param name="path">The path.</param>
        /// <returns>The shorten path.</returns>
        /// <contract>
        ///   <requires name="path" condition="not null" />
        ///   <ensures condition="nullable" />
        /// </contract>
        private string ShortenPath(string path)
        {
            Assert.ArgumentNotNull((object)path, nameof(path));
            Item root = this.MediaDataContext.GetRoot();
            Assert.IsNotNull((object)root, "root");
            Item rootItem = root.Database.GetRootItem();
            Assert.IsNotNull((object)rootItem, "database root");
            if (root.ID != rootItem.ID)
            {
                string path1 = root.Paths.Path;
                if (path.StartsWith(path1, StringComparison.InvariantCulture))
                    path = StringUtil.Mid(path, path1.Length);
            }
            return path;
        }
    }

}
