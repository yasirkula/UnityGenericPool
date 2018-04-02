# UnityGenericPool
A pooling solution for Unity3D that can store "almost" anything; ranging from Unity objects (e.g. Component, GameObject, Texture) to plain C# objects. It can't store structs but since they are pass-by-value, that would be pointless.

This pool comes with a helper class (SimplePoolHelper) that provides global access to pools that can optionally be named. It is no longer necessary to keep references to pools in your scripts (however, if you wish, you can ignore SimplePoolHelper and manage your pools manually). Note that SimplePoolHelper pools are stored statically and are persistent between scenes.

# Method Signatures
```C#
SimplePool<T>:

// blueprint: object that gets instantiated when the pool is empty (only if T is of type UnityEngine.Object); also passed as parameter to the CreateFunction
// CreateFunction: called when the pool needs to be populated. Takes blueprint as parameter and should return a new object (if left null and if T is of type UnityEngine.Object, blueprint will be instantiated)
// OnPush: called when an object is pushed to the pool; can be used to e.g. deactivate the object
// OnPop: called when an object is popped from the pool; can be used to e.g. activate the object
SimplePool( Func<T, T> CreateFunction = null, Action<T> OnPush = null, Action<T> OnPop = null );
SimplePool( T blueprint, Func<T, T> CreateFunction = null, Action<T> OnPush = null, Action<T> OnPop = null );

bool Populate( int count );
bool Populate( T blueprint, int count );
T Pop();
T[] Pop( int count );
void Push( T obj );
void Push( IEnumerable<T> objects );
void Clear( bool destroyObjects = true );

SimplePoolHelper:

static SimplePool<T> GetPool<T>( string poolName = null );
static void Push<T>( T obj, string poolName = null );
static T Pop<T>( string poolName = null );
static void Pool<T>( this T obj, string poolName = null );
```

# Example Code
```C#
using UnityEngine;

public class PlayerWeapons : MonoBehaviour 
{
	public Transform bulletPrefab;
	public Transform grenadePrefab;

	// Called when the scene starts
	void Awake()
	{
		// Get a Transform pool called Bullets
		SimplePool<Transform> bulletsPool = SimplePoolHelper.GetPool<Transform>( "Bullets" );

		// Get another Transform pool called Grenades
		SimplePool<Transform> grenadesPool = SimplePoolHelper.GetPool<Transform>( "Grenades" );

		// When a bullet is pooled, deactivate it
		bulletsPool.OnPush = ( item ) => item.gameObject.SetActive( false );

		// When a bullet is fetched from the pool, activate it and set its position
		bulletsPool.OnPop = ( item ) =>
		{
			item.gameObject.SetActive( true );
			item.position = transform.position;
			item.rotation = transform.rotation;
		};
		
		// When pool tries to create a new bullet (when empty), create an instance of bulletPrefab and keep it alive between scenes
		bulletsPool.CreateFunction = ( template ) => 
		{
			Transform newBullet = Instantiate( bulletPrefab );
			DontDestroyOnLoad( newBullet );
			return newBullet;
		};

		// When a grenade is pooled, deactivate it
		grenadesPool.OnPush = ( item ) => item.gameObject.SetActive( false );
		// When a grenade is fetched from the pool, activate it (you can also set its position here, it is entirely up to you)
		grenadesPool.OnPop = ( item ) => item.gameObject.SetActive( true );
		// When pool tries to create a new grenade, simply create an instance of grenadePrefab
		grenadesPool.CreateFunction = ( template ) => Instantiate( grenadePrefab );

		// Populate the pool with 10 bullet instances (optional)
		bulletsPool.Populate( 10 );

		// Populate the pool with 4 grenade instances (optional)
		grenadesPool.Populate( 4 );

		// Yet another pool that will store some WaitForSeconds instances for reuse in Grenade class
		// This pool is different from the previous pools in two ways:
		// 1) it stores a plain object: WaitForSeconds, which is not a Component nor a GameObject
		// 2) it has no name since there is only one pool of type WaitForSeconds; note that
		//    it is faster and more efficient to access an unnamed pool in SimplePoolHelper
		// CreateFunction: simply create a new WaitForSeconds instance that waits for 6 seconds
		// There is no need to do any special operations on OnPush or OnPop, so they are not altered
		SimplePoolHelper.GetPool<WaitForSeconds>().CreateFunction = ( template ) => new WaitForSeconds( 6f );
	}

	// Called when the object is destroyed
	void OnDestroy()
	{
		// This transform is about to be destroyed, bullets fetched from pool can no longer use it
		// So fetched bullets are simply activated, without changing their position
		SimplePoolHelper.GetPool<Transform>( "Bullets" ).OnPop = ( item ) => item.gameObject.SetActive( true );

		// Grenades in the pool are not persistent between scenes (unlike bullets), and will become null references
		// So clear all grenade instances in the Grenades pool
		SimplePoolHelper.GetPool<Transform>( "Grenades" ).Clear();
	}

	void Update()
	{
		// Left mouse button clicked: fire a bullet
		if( Input.GetMouseButtonDown( 0 ) )
		{
			// Add forward force to the bullet
			SimplePoolHelper.Pop<Transform>( "Bullets" ).GetComponent<Rigidbody>().velocity = transform.forward * 100f;
		}

		// Right mouse button clicked: throw a grenade
		if( Input.GetMouseButtonDown( 1 ) )
		{
			Transform grenade = SimplePoolHelper.Pop<Transform>( "Grenades" );

			// Set the position of the grenade (unlike bullets, it is not automatically set in Pop)
			grenade.position = transform.position;

			// Add forward force to the grenade (also some upwards force as well)
			grenade.GetComponent<Rigidbody>().velocity = transform.forward * 30f + transform.up * 15f;
		}
	}
}
```

```C#
using UnityEngine;

public class Bullet : MonoBehaviour 
{
	// Pool the bullet if it doesn't hit anything in 5 seconds
	void OnEnable()
	{
		Invoke( "PoolBullet", 5f );
	}

	// Bullet is deactivated (probably hit something and got pooled), cancel the PoolBullet invoke just in case
	void OnDisable()
	{
		CancelInvoke( "PoolBullet" );
	}
	
	void OnCollisionEnter( Collision other )
	{
		// Apply damage to the collided entity
		Health entityHealth = other.transform.GetComponent<Health>();
		if( entityHealth != null )
			entityHealth.ApplyDamage( 10 );

		// Pool the bullet instead of destroying it 
		// As bullets are kept in a Transform pool, the Pool extension function is used on the transform of the pool
		transform.Pool( "Bullets" );

		// Alternative pool method (does exactly the same thing as Pool function)
		// SimplePoolHelper.Push( transform, "Bullets" );
	}

	private void PoolBullet()
	{
		transform.Pool( "Bullets" );
	}
}
```

```C#
using UnityEngine;
using System.Collections;

public class Grenade : MonoBehaviour 
{
	// Explode after 6 seconds
	// Notice how we use OnEnable instead of Start as Start is only called once during the lifetime of the object
	void OnEnable()
	{
		StartCoroutine( Explode() );
	}

	IEnumerator Explode()
	{
		// Fetch an instance of WaitForSeconds from the pool
		WaitForSeconds waitForSeconds = SimplePoolHelper.Pop<WaitForSeconds>();

		// Wait for 6 seconds (set in PlayerWeapons class)
		yield return waitForSeconds;

		// Pool the WaitForSeconds instance for reuse
		waitForSeconds.Pool();

		// Play some fancy particles
		GetComponent<ParticleSystem>().Emit( 50 );

		// Pool the grenade for reuse
		transform.Pool( "Grenades" );
	}
}
```
