using BEditor.Data;
using BEditor.Data.Primitive;
using BEditor.Data.Property;
using BEditor.Drawing;
using BEditor.Drawing.Pixel;
using BEditor.Graphics;
using BEditor.Media;

using System.Collections.Generic;
using System.Numerics;

namespace BEditorPluginSamples
{
    public class MultipleObject : ImageObject
    {
        public override string Name => "多重画像";

        public override IEnumerable<PropertyElement> GetProperties()
        {
            // プロパティを持たない場合
            // "yield break;" or "return Array.Empty<PropertyElement>();" or "return Enumerable.Empty<PropertyElement>();"

            yield return Coordinate;
            yield return Scale;
            yield return Blend;
            yield return Rotate;
            yield return Material;
        }

        protected override Image<BGRA32> OnRender(EffectApplyArgs args)
        {
            // void OnRender(EffectApplyArgs<IEnumerable<Texture>> args) を実装していても呼び出される可能性があるので
            // 例外は投げないでください
            return new Image<BGRA32>(1, 1, (BGRA32)default);
        }

        protected override void OnRender(EffectApplyArgs<IEnumerable<Texture>> args)
        {
            args.Value = Render(args.Frame);
        }

        private IEnumerable<Texture> Render(Frame frame)
        {
            // サイズ
            var size = 100;
            var half = size / 2;
            var quarter = half / 2;

            // 円を作成
            var circle = Image.Ellipse(size, size, size, Colors.White);
            // 左上
            using var upperLeft = circle[new Rectangle(0, 0, half, half)];
            // 右上
            using var upperRight = circle[new Rectangle(half, 0, half, half)];
            // 左下
            using var lowerLeft = circle[new Rectangle(0, half, half, half)];
            // 右下
            using var lowerRight = circle[new Rectangle(half, half, half, half)];

            // 右下に移動
            yield return CreateFromImage(upperLeft, frame, new(quarter, -quarter, 0));

            // 左下に移動
            yield return CreateFromImage(upperRight, frame, new(-quarter, -quarter, 0));

            // 右上に移動
            yield return CreateFromImage(lowerLeft, frame, new(quarter, quarter, 0));

            // 左上に移動
            yield return CreateFromImage(lowerRight, frame, new(-quarter, quarter, 0));
        }

        // 画像からテクスチャを作成
        private Texture CreateFromImage(Image<BGRA32> image, Frame frame, Vector3 rel)
        {
            var texture = Texture.FromImage(image);

            // Materialを設定
            var ambient = Material.Ambient[frame];
            var diffuse = Material.Diffuse[frame];
            var specular = Material.Specular[frame];
            var shininess = Material.Shininess[frame];
            texture.Material = new(ambient, diffuse, specular, shininess);

            // Blend を設定
            var alpha = Blend.Opacity[frame] / 100f;
            var color = Blend.Color[frame];
            color.A = (byte)(color.A * alpha);
            texture.Color = color;
            texture.BlendMode = (BlendMode)Blend.BlendType.Index;

            // Transformを設定
            texture.Transform += GetTransform(frame);

            // Transformはstructなので、
            // "texture.Transform.Relative = rel;"
            // はできない
            var transform = texture.Transform;
            transform.Relative = rel;
            texture.Transform = transform;

            return texture;
        }
    }
}
