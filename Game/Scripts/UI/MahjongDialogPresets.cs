using System.Collections;
using System.Collections.Generic;
using Template.Systems.Dialogs;
using Template.Systems.Dialogs.Elements;
using Template.UIBase;
using UnityEngine;

namespace GameCore.UI
{
    public class MahjongDialogPresets
    {
        // Shortcuts
        private static UIDialogText Header => DialogElementCollection.Instance.HeaderPrefab.GetInstanceFromPool();
        private static UIDialogContainer TextSection => DialogElementCollection.Instance.ContainerPrefab.GetInstanceFromPool();
        private static UIDialogContainer DialogBackground => DialogElementCollection.Instance.BackgroundPrefab.GetInstanceFromPool();
        private static UIDialogText Text => DialogElementCollection.Instance.TextPrefab.GetInstanceFromPool();
        private static UIDialogContainer ButtonPanel => DialogElementCollection.Instance.ButtonPanelPrefab.GetInstanceFromPool();
        private static UIDialogFinishButton ResolveButton => DialogElementCollection.Instance.FinishButtonPrefab.GetInstanceFromPool();
        private static UIDialogContentButton ContentButton => DialogElementCollection.Instance.ContentButtonPrefab.GetInstanceFromPool();
        private static UIDialogFinishButton AdsContentButton => DialogElementCollection.Instance.AdsFinishButtonPrefab.GetInstanceFromPool();
        private static UIDialogSprite Sprite => DialogElementCollection.Instance.SpritePrefab.GetInstanceFromPool();
        private static Color WarningRed => StyleCollection.Instance.colorCollection.WarningRed;
        private static Color White => StyleCollection.Instance.colorCollection.White;

        public static void InitAsAdsDialog(UIDialog dialog, string header, string text, bool useTextFrame, string adsButtonText)
        {
            var elements = new List<UIDialogElementBase>();

            if (!string.IsNullOrEmpty(header))
            {
                elements.Add(Header.WithText(header));
            }

            if (!string.IsNullOrEmpty(text))
            {
                elements.Add(useTextFrame ?
                    TextSection.WithContent(new UIDialogElementBase[] { Text.WithText(text).WithFontSize(44) }) :
                    Text.WithText(text).WithFontSize(44) as UIDialogElementBase);
            }

            if (!string.IsNullOrEmpty(adsButtonText))
            {
                elements.Add(ButtonPanel.WithContent(new[]
                                            {
                                              ResolveButton.WithText(adsButtonText + "<br><material=\"CeraRoundPro-Bold SDF Outline Brown\">50 <sprite=\"coinIcon\" index=1>").WithResult(new Ok_DialogButton())
                                              //ResolveButton.WithText("Common.Exit".Localized()).WithResult(new Close_DialogButton()).WithPreset(ButtonViewPreset.brown)
                                            }));
                elements.Add(ButtonPanel);
                elements.Add(ButtonPanel.WithContent(new[]
                                            {
                                              //ResolveButton.WithText("<sprite=\"adsIcon\" index=0>"+ adsButtonText).WithResult(new Ok_DialogButton()),
                                              ResolveButton.WithText("Common.Exit".Localized()).WithResult(new Close_DialogButton()).WithPreset(ButtonViewPreset.brown)
                                            }));
            }

            dialog.SetContent(new[]
            {
                DialogBackground.WithContent(elements)
            });

            if (string.IsNullOrEmpty(adsButtonText))
            {
                dialog.SetBackgroundPressResult(new Close_DialogButton());
            }
        }

        public static void InitAsEndGameDialog(UIDialog dialog)
        {
            dialog.SetContent(new[]
               {
                DialogBackground.WithContent(new UIDialogElementBase[]
                {
                    Header.WithText("Common.EndGame.Title".Localized()),
                    TextSection.WithContent(new UIDialogElementBase[] { Text.WithText("Common.EndGame.Text".Localized()).WithFontSize(55) }),
                    ButtonPanel.WithContent(new []
                    {
                        ResolveButton.WithText("Common.LeaveReview".Localized()).WithResult(new Ok_DialogButton()),
                        ResolveButton.WithText("Common.OK".Localized()).WithResult(new Close_DialogButton()).WithPreset(ButtonViewPreset.brown)
                    })
                })
            });
        }

        public static void InitAsReviewDialog(UIDialog dialog)
        {
            dialog.SetContent(new[]
               {
                DialogBackground.WithContent(new UIDialogElementBase[]
                {
                    //Header.WithText("Common.EndGame.Title".Localized()),
                    TextSection.WithContent(new UIDialogElementBase[] { Text.WithText("Common.LeaveReview.Text".Localized()).WithFontSize(55) }),
                    ButtonPanel.WithContent(new []
                    {
                        ResolveButton.WithText("Common.LeaveReview".Localized()).WithResult(new Ok_DialogButton()),
                        ResolveButton.WithText("Common.OK".Localized()).WithResult(new Close_DialogButton()).WithPreset(ButtonViewPreset.brown)
                    })
                })
            });
        }

        public static void InitAsBlockedTiles(UIDialog dialog)
        {
            dialog.SetContent(new[]
               {
                DialogBackground.WithContent(new UIDialogElementBase[]
                {
                    TextSection.WithContent(new UIDialogElementBase[] { Text.WithText("Classic/Common.TileBlocked".Localized()).WithFontSize(55) }),
                    Sprite.WithImage(Resources.Load<Sprite>("tutor_blocked_tile")).WithSize(new Vector2(357f, 248f)),
                    ButtonPanel.WithContent(new []
                    {
                        ResolveButton.WithText("Common.OK".Localized()).WithResult(new Ok_DialogButton())
                    })
                })
            });
        }

        public static void InitAsFlowerTiles(UIDialog dialog)
        {
            dialog.SetContent(new[]
               {
                DialogBackground.WithContent(new UIDialogElementBase[]
                {
                    TextSection.WithContent(new UIDialogElementBase[] { Text.WithText("Classic/Common.Macthing.Flowers".Localized()).WithFontSize(55) }),
                    Sprite.WithImage(Resources.Load<Sprite>("flowers")).WithSize(new Vector2(296f, 353f)),
                    ButtonPanel.WithContent(new []
                    {
                        ResolveButton.WithText("Common.OK".Localized()).WithResult(new Ok_DialogButton())
                    })
                })
            });
        }

        public static void InitAsSeasonTiles(UIDialog dialog)
        {
            dialog.SetContent(new[]
               {
                DialogBackground.WithContent(new UIDialogElementBase[]
                {
                    TextSection.WithContent(new UIDialogElementBase[] { Text.WithText("Classic/Common.Matching.Seasons".Localized()).WithFontSize(55) }),
                    Sprite.WithImage(Resources.Load<Sprite>("seasons")).WithSize(new Vector2(296f, 353f)),
                    ButtonPanel.WithContent(new []
                    {
                        ResolveButton.WithText("Common.OK".Localized()).WithResult(new Ok_DialogButton())
                    })
                })
            });
        }
    }
}
