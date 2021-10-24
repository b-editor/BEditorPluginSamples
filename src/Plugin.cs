using BEditor;
using BEditor.Data;
using BEditor.Plugin;

using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

using static BEditorPluginSamples.PluginSettings;

namespace BEditorPluginSamples
{
    public class Plugin : PluginObject
    {
        private SettingRecord? _settings;

        public Plugin(PluginConfig config)
            : base(config)
        {
        }

        public override string PluginName => "BEditorPluginSamples";

        public override string Description => "プラグインのサンプル";

        public override Guid Id { get; } = Guid.Parse("{EBD7CB04-3428-49AF-A85C-5FE7197C6E44}");

        public override SettingRecord Settings
        {
            get
            {
                if (_settings == null)
                {
                    // 設定がnullの場合ファイルから読み込む、
                    // SettingRecord.LoadFromからnullが返ってきたらデフォルトの設定をNewする。
                    _settings = SettingRecord.LoadFrom<PluginSettings>(Path.Combine(BaseDirectory, "settings.json"))
                        ?? new PluginSettings("文字列を入力", 100, Numbers.One);
                }

                return _settings;
            }
            set
            {
                _settings = value;
                // 設定を保存
                _settings.Save(Path.Combine(BaseDirectory, "settings.json"));
            }
        }

        public static void Register()
        {
            PluginBuilder.Configure<Plugin>()
                .With(ObjectMetadata.Create<MultipleObject>("多重画像"))
                .Task(new PluginTask(PluginLoadingTask, "プラグインを読み込んでいます。"))
                .Register();
        }

        private static async ValueTask PluginLoadingTask(IProgressDialog dialog)
        {
            dialog.Maximum = 5;

            for (int i = 5 - 1; i >= 0; i--)
            {
                dialog.Text = $"あと {i}秒";
                await Task.Delay(1000);

                dialog.Value = 5 - i;
            }
        }
    }

    public record PluginSettings(
        [property: DisplayName("文字列")]
        string String,
        [property: DisplayName("数値")]
        int Number,
        [property: DisplayName("列挙型")]
        Numbers Enum)
        : SettingRecord
    {
        public enum Numbers
        {
            One,
            Two,
            Three,
        }
    }
}