using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BaseRoot : MonoBehaviour
{
    public RootData type;
    public List<Collider2D> requiredPlacements;
    [HideInInspector] public bool isPlaced;
    [SerializeField] List<SpriteRenderer> sprites;

    private List<MeshRenderer> meshes;
    private List<Color> defaultSpriteColours;
    private List<Color> defaultMeshColours;
    private bool valid = true;
    private List<ParticleSystem> particles;

    private void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>().ToList();

        foreach( var particle in particles )
            particle.Stop( true, ParticleSystemStopBehavior.StopEmittingAndClear );

        defaultSpriteColours = new List<Color>();
        defaultSpriteColours.Capacity = sprites.Count;
        foreach( var sprite in sprites )
            defaultSpriteColours.Add( sprite.color );

        defaultMeshColours = new List<Color>();
        meshes = new List<MeshRenderer>();

        // Delay for the mesh case because we need to wait for the spline mesh to be generated
        Utility.FunctionTimer.CreateTimer( 0.1f, () =>
        {
            meshes = GetComponentsInChildren<MeshRenderer>().ToList();
            defaultMeshColours.Capacity = meshes.Count;
            foreach( var mesh in meshes )
                defaultMeshColours.Add( mesh.material.GetColor( "_Colour" ) );

            HighlightValidPlacement( valid );
        } );
    }

    public void HighlightValidPlacement( bool valid )
    {
        this.valid = valid;

        foreach( var( sprite, color ) in sprites.Zip( defaultSpriteColours ) )
        {
            sprite.color = valid ? color : GameController.Instance.Constants.invalidPlacementColour;
        }

        foreach( var (mesh, color) in meshes.Zip( defaultMeshColours ) )
        {
            mesh.material.SetColor( "_Colour", valid ? color : GameController.Instance.Constants.invalidPlacementColour );
        }
    }

    public virtual void OnPlacement() 
    {
        foreach( var particle in particles )
            particle.Play();
    }
}