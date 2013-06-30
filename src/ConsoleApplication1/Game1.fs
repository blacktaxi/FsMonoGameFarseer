
namespace Game

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Storage
open Microsoft.Xna.Framework.GamerServices

open FarseerPhysics

module Utils =
    let inline nil () = Unchecked.defaultof<_>
open Utils

type Game1 () as this =
    inherit Game ()

    [<Literal>]
    let text = """Press A or D to rotate the ball
                Press Space to jump
                Use arrow keys to move the camera"""

    do 
        this.Content.RootDirectory <- "Content"
    let graphics = new GraphicsDeviceManager(this)
    let world = new Dynamics.World(new Vector2(0.0f, 9.82f))

    let mutable view = nil ()
    let mutable cameraPosition = nil ()
    let mutable screenCenter = nil ()
    let mutable batch = nil ()
    let mutable font = nil ()
    let mutable circleSprite = nil ()
    let mutable groundSprite = nil ()
    let mutable circleBody = nil ()
    let mutable groundBody = nil ()
    let mutable circleOrigin = nil ()
    let mutable groundOrigin = nil ()

    override this.LoadContent () =
        view <- Matrix.Identity
        cameraPosition <- Vector2.Zero
        screenCenter <- new Vector2(float32 graphics.GraphicsDevice.Viewport.Width / 2.0f, 
                                    float32 graphics.GraphicsDevice.Viewport.Height / 2.0f)
        batch <- new SpriteBatch(graphics.GraphicsDevice)

        font <- this.Content.Load<SpriteFont>("font")
        circleSprite <- this.Content.Load<Texture2D>("circleSprite")
        groundSprite <- this.Content.Load<Texture2D>("groundSprite")
        groundOrigin <- new Vector2(float32 groundSprite.Width / 2.0f, float32 groundSprite.Height / 2.0f)
        circleOrigin <- new Vector2(float32 circleSprite.Width / 2.0f, float32 circleSprite.Height / 2.0f)

        ConvertUnits.SetDisplayUnitToSimUnitRatio(64.0f)

        circleBody <- Factories.BodyFactory.CreateCircle(
            world, 
            ConvertUnits.ToSimUnits(96.0f / 2.0f), 
            1.0f, 
            ConvertUnits.ToSimUnits(screenCenter) + new Vector2(0.0f, -1.5f))
        circleBody.BodyType <- Dynamics.BodyType.Dynamic
        circleBody.Restitution <- 0.3f
        circleBody.Friction <- 0.5f

        groundBody <- Factories.BodyFactory.CreateRectangle(
            world, 
            ConvertUnits.ToSimUnits(512.0f), 
            ConvertUnits.ToSimUnits(64.0f), 
            1.0f, 
            ConvertUnits.ToSimUnits(screenCenter) + new Vector2(0.0f, 1.25f))
        groundBody.IsStatic <- true
        groundBody.Restitution <- 0.3f
        groundBody.Friction <- 0.3f




    override this.Update (gameTime) = 
        if GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed 
            || Keyboard.GetState().IsKeyDown(Keys.Escape) then
            this.Exit();

        let kbState = Keyboard.GetState()

        if kbState.IsKeyDown(Keys.Left) then cameraPosition.X <- cameraPosition.X + 1.5f
        if kbState.IsKeyDown(Keys.Right) then cameraPosition.X <- cameraPosition.X - 1.5f
        if kbState.IsKeyDown(Keys.Up) then cameraPosition.Y <- cameraPosition.Y + 1.5f
        if kbState.IsKeyDown(Keys.Down) then cameraPosition.Y <- cameraPosition.Y - 1.5f

        view <- Matrix.CreateTranslation(new Vector3(cameraPosition - screenCenter, 0.0f)) * 
            Matrix.CreateTranslation(new Vector3(screenCenter, 0.0f));

        if kbState.IsKeyDown(Keys.A) then circleBody.ApplyTorque(-20.0f)
        if kbState.IsKeyDown(Keys.D) then circleBody.ApplyTorque(20.0f)

        if kbState.IsKeyDown(Keys.Space) then
            circleBody.ApplyLinearImpulse(new Vector2(0.0f, -1.0f))

        world.Step <| float32 gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f

        base.Update(gameTime)

    override this.Draw (gameTime) =
        this.GraphicsDevice.Clear (Color.CornflowerBlue)

        let inline n () = new System.Nullable<_>()

        batch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, view)
        batch.Draw(
            circleSprite, 
            ConvertUnits.ToDisplayUnits(circleBody.Position), 
            n (), 
            Color.White, 
            circleBody.Rotation, 
            circleOrigin, 
            1.0f, 
            SpriteEffects.None, 
            0.0f)

        batch.Draw(
            groundSprite, 
            ConvertUnits.ToDisplayUnits(groundBody.Position), 
            n (), 
            Color.White, 
            0.0f, 
            groundOrigin, 
            1.0f, 
            SpriteEffects.None, 
            0.0f)

        batch.End()

        // Display instructions
        batch.Begin()
        batch.DrawString(font, text, new Vector2(14.0f, 14.0f), Color.Black)
        batch.DrawString(font, text, new Vector2(12.0f, 12.0f), Color.White)
        batch.End()

        base.Draw(gameTime)
        