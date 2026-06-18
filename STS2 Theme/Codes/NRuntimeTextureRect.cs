using Godot;
using MegaCrit.Sts2.Core.Logging;

public partial class NRuntimeTextureRect : TextureRect
{
    public override void _Ready()
    {
        if (this.Owner != null && ResourceHandler.TryGet(NodeUtil.BuildID(this), out var path))
        {
            var image = new Image();
            var err = image.Load(path);
            if (err != Error.Ok)
            {
                Log.Error($"[>>>STS2 Theme]Failed to load TextureImage: {path}");
                return;
            }

            var tex = ImageTexture.CreateFromImage(image);
            Texture = tex;
        }
    }
}