# Adding Enemies

WeaverCore provides an easy way for both creating and testing new enemies and bosses.

To get started, first add in a template enemy via WeaverCore -> Insert -> Template Enemy.

![Template Enemy](~/images/templateEnemy.PNG)

This will insert a template enemy that has most of the components we need to build our new enemy:

| Component  | Description |
| ------------- | ------------- |
| Sprite Renderer | Used for rendering a face sprite on the enemy  |
| [Sprite Flasher](xref:WeaverCore.Components.SpriteFlasher)  | Causes the sprite to flash a certain color when hit  |
| [Player Damager](xref:WeaverCore.Components.PlayerDamager)  | Causes the player to take damage when colliding with the enemy  |
| Box Collider 2D  | Allows other objects, like the player, to come in contact with this enemy  |
| Rigidbody 2D | Allows the enemy to interact with it's enviroment, and have gravity applied to it  |
| [Entity Health](xref:WeaverCore.Components.EntityHealth)  | Keeps track of how much health the enemy has left  |
| [Recoiler](xref:WeaverCore.Components.Recoiler)  | Used to recoil the enemy backwards when the player hits it  |
| Enemy Dreamnail Reaction  | Allows the player to dreamnail this enemy for soul. This component can also be customized to display a message when dreamnailed  |

You can find more information about these components by going over to the API Documentation.

These components have most of what we need for the enemy to function, but there are a few missing ones that we need to fill in ourselves.

# Hit Effects

What kind of effects should be played when the enemy gets hit? In WeaverCore, there are two main kinds of hit effects:

1. The [HitEffectsNormal](xref:WeaverCore.Components.HitEffects.HitEffectsNormal) component, for enemies that aren't infected, like Hornet or Grimm
2. The [HitEffectsInfected](xref:WeaverCore.Components.HitEffects.HitEffectsInfected) component, for enemies that are infected

Be sure to add either one of these components to your enemy, or you can build your own by creating a component that inherits from [IHitEffects](xref:WeaverCore.Interfaces.IHitEffects). Not having Hit Effects component will cause the enemy to emit nothing when hit

# Death effects

What kind of effects should be played when the enemy dies? Just like with HitEffects, there are two main kinds of Death Effects:

1. The [UninfectedDeathEffects](xref:WeaverCore.Components.DeathEffects.UninfectedDeathEffects) component, for enemies that aren't infected
2. The [InfectedDeathEffects](xref:WeaverCore.Components.DeathEffects.InfectedDeathEffects) component, for enemies that are infected

Be sure to have either one of these components attached to have death effects play for the enemy, or you can build your own by creating a script that inherits from [IDeathEffects](xref:WeaverCore.Interfaces.IDeathEffects) or [BasicDeathEffects](xref:WeaverCore.Components.DeathEffects.BasicDeathEffects)

![Hit Effects Added](~/images/hitEffectsAdded.PNG)

# Enemy Logic

The final thing to do is to create a new script that inherits from [Enemy](xref:WeaverCore.Features.Enemy), which will house the main logic for our Enemy, and add it to the Enemy Object:

```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Features;

public class CustomEnemy : Enemy
{

}
```

![Add Enemy Script](~/images/addEnemyScript.gif)

For this tutorial, we will create an enemy that has two moves, a slide move, and a lunge move. The slide move will slide on the ground to move towards the player, and the lunge move will do a lunge towards the player.

In WeaverCore, we can split these two moves into their own objects by creating two classes that inherit from [IEnemyMove](xref:WeaverCore.Features.Enemy)

```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Interfaces;

//CustomEnemy.cs
public class CustomEnemy : Enemy
{
    void Start()
	{

	}
}

//SlideMove.cs
public class SlideMove : MonoBehaviour, IEnemyMove
{
    public bool MoveEnabled => true;

    public IEnumerator DoMove()
    {
        yield break;
    }

    public void OnCancel()
    {

    }

    public void OnDeath()
    {

    }
}

//LungeMove.cs
public class LungeMove : MonoBehaviour, IEnemyMove
{
    public bool MoveEnabled => true;

    public IEnumerator DoMove()
    {
        yield break;
    }

    public void OnCancel()
    {

    }

    public void OnDeath()
    {

    }
}
```

[IEnemyMove](xref:WeaverCore.Features.Enemy) has 4 major parts to it:
1. **MoveEnabled** is used to tell if the move is active or not
2. **DoMove** contains the move's main functionality
3. **OnCancel** is called when the move is abruptly cancelled
4. **OnDeath** is called when the enemy dies (looses all of it's health)

> [!NOTE]
> You don't need to have the moves inherit from **MonoBehaviour**, but for this tutorial, we will be using it so that we can add the moves to the enemy object to make it easier to configure the moves

We can now add these new moves to the enemy object:

![Add Move Components](~/images/addingMoveComponents.gif)


In the CustomEnemy Start() function, we can then use GetComponents<IEnemyMove>() to get both of these moves attached on the object:

```cs
using System.Collections.Generic;
using WeaverCore.Features;
using WeaverCore.Interfaces;

public class CustomEnemy : Enemy
{
    IEnemyMove[] moves;

    void Start()
    {
        moves = GetComponents<IEnemyMove>();
    }
}
```

Now we need to decide how we want to run our moves. Do we want the moves to be done randomly? Do we want them to be run in an alternating pattern? Do we also want to have a delay before the next move gets run?

For this tutorial, we will simply run the moves in an alternating pattern, and there will also be a 0.5 second delay before the next move gets run.

We can do this by first starting up a coroutine function that will house our code for running the moves:

```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Interfaces;

public class CustomEnemy : Enemy
{
    IEnemyMove[] moves;

    Coroutine mainRoutine;

    void Start()
    {
        moves = GetComponents<IEnemyMove>();

		//Start the main routine for executing the moves
        mainRoutine = StartCoroutine(MainRoutine());
    }

    IEnumerator MainRoutine()
    {
        //Loop forever
        while (true)
        {
			//Loop over each of the moves in the moves list
			//First the Lunge Move, then the Slide Move
            foreach (var move in moves)
            {
				//Run the move
                yield return RunMove(move);

				//Wait 0.5 seconds before going to the next move
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
```

What this code will do is start up the MainRoutine() when the enemy starts up, and the routine will continuously alternate between the Lunge Move and the Slide move, with a 0.5 second delay between each move.

The [Enemy](xref:WeaverCore.Features.Enemy) class provides a function called [RunMove](xref:WeaverCore.Features.Enemy.RunMove(WeaverCore.Interfaces.IEnemyMove)) which will execute the move for us. This function will also handle the OnCancel and OnDeath callbacks that [IEnemyMove](xref:WeaverCore.Features.Enemy) requires, so you should always use this function when you want to execute a move.

One issue with this code is that if the enemy dies, this coroutine doesn't stop running. The enemy will continue to slide and lunge towards the player even if the enemy has lost all of it's health. There are two ways this can be fixed:

### Fix 1 : StopCoroutine

The first way to stop the coroutine when the enemy dies is to override the [OnDeath](xref:WeaverCore.Features.Enemy.OnDeath) function, which is called when the enemy dies. In this function, we can make a call to StopCoroutine to stop the MainRoutine from executing any moves after the enemy dies:

```cs
	...
    IEnumerator MainRoutine()
    {
		//Loop forever
        while (true)
        {
            foreach (var move in moves)
            {
                yield return RunMove(move);

				//Wait 0.5 seconds before going to the next move
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
	//--//--//--//--//--//--//--//--//--//--//---
	//When the enemy dies, stop the main routine from executing any more moves
    protected override void OnDeath()
    {
        StopCoroutine(mainRoutine);
    }
	//--//--//--//--//--//--//--//--//--//--//---
```

### Fix 2 : StartBoundRoutine

The other method is to instead use [StartBoundRoutine](xref:WeaverCore.Features.Enemy.StartBoundRoutine(System.Collections.IEnumerator)) provided by the [Enemy](xref:WeaverCore.Features.Enemy) class, instead of StartCoroutine. What StartBoundRoutine does is that it will start up a routine like normal, but when the enemy dies, the coroutine is automatically stopped:

```cs
    IEnemyMove[] moves;

	uint mainRoutineID;

    void Start()
    {
        moves = GetComponents<IEnemyMove>();

		//--//--//--//--//--//--//--//--
        mainRoutineID = StartBoundRoutine(MainRoutine());
		//--//--//--//--//--//--//--//--
    }
```

## Implementing the Moves - Slide Move

Now It's time to implement the individual moves. We will start with creating the Slide Move. When the move is executed, it will slide along the ground in the direction of the player. It will do this for about 10 seconds before stopping:

```cs
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class SlideMove : MonoBehaviour, IEnemyMove
{
    [SerializeField]
    float moveSpeed = 5f;

    [SerializeField]
    float moveDuration = 10;

    public bool MoveEnabled => true;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public IEnumerator DoMove()
    {
        //Run the Move() function every frame until 10 seconds have elapsed
        yield return CoroutineUtilities.RunForPeriod(moveDuration, Move);

        //Stop moving when finished
        rb.velocity = Vector2.zero;
    }

    void Move()
    {
        //If the player is to the right of the enemy
        if (Player.Player1.transform.position.x >= transform.position.x)
        {
            //Set the horizontal velocity to move to the right
            rb.velocity = rb.velocity.With(x: moveSpeed);
        }
        //Otherwise, if the player is to the left of the enemy
        else
        {
            //Set the horizontal velocity to move to the left
            rb.velocity = rb.velocity.With(x: -moveSpeed);
        }
    }

    public void OnCancel()
    {
        //Stop moving if the move is cancelled
        rb.velocity = Vector2.zero;
    }

    public void OnDeath()
    {
        //Stop moving if the enemy dies while this move is running
        rb.velocity = Vector2.zero;
    }
}
```

You can read over the commented code to get an idea of what's going on, but there are a few new functions here:

1. [CoroutineUtilities.RunForPeriod](xref:WeaverCore.Utilities.CoroutineUtilities.RunForPeriod(System.Single,System.Action)) is used to run a certain function every frame for a certain amount of time.
2. [Player.Player1](xref:WeaverCore.Player.Player1) is used to get the main player in the game.
3. The [With](xref:WeaverCore.Utilities.VectorUtilities.With(UnityEngine.Vector2,System.Single,System.Single)) extension method is useful if we only want to modify one field of a vector. In this case, we are using it to create a new vector with only the "X" field modified

## Implementing the Moves - Lunge Move

Now for the Lunge Move. This move will cause the enemy to lunge towards the player, and the move will end when the enemy touches the ground:

```cs
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class LungeMove : MonoBehaviour, IEnemyMove
{
    public bool MoveEnabled => true;

    Rigidbody2D rb;
    GroundDetector groundDetector;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundDetector = GetComponent<GroundDetector>();
		//If there is no Ground Detector on the object, then add one
        if (groundDetector == null)
        {
            groundDetector = gameObject.AddComponent<GroundDetector>();
        }
    }

    public IEnumerator DoMove()
    {
        var startPos = transform.position;
        var destPos = Player.Player1.transform.position;
        float time = 0.6f;


        //Calculate the initial velocity needed to get from the current position to the player's position in 0.6 seconds
        var lungeVelocity = MathUtilties.CalculateVelocityToReachPoint(startPos, destPos, time);

        //Set the velocity to start the lunge
        rb.velocity = lungeVelocity;

        //Wait for 0.6 seconds
        yield return new WaitForSeconds(time);

        //Wait until the enemy is touching the ground
        yield return new WaitUntil(() => groundDetector.TouchingGround);
    }

    public void OnCancel() { }

    public void OnDeath() { }
}

```

This move makes use of [MathUtilties.CalculateVelocityToReachPoint](xref:WeaverCore.Utilities.MathUtilties.CalculateVelocityToReachPoint(UnityEngine.Vector2,UnityEngine.Vector2,System.Double,System.Double)), which allows us to calculate the initial velocity needed to travel from a start to end pososition in a certain amount of time.

We also make use of the [GroundDetector](xref:WeaverCore.Components.GroundDetector) component to wait until the enemy is on the ground before finishing the move.

## Testing our Enemy

With the individual moves now implemented, we can now test out our enemy. Before we do that though, we should add some ground so the enemy doesn't fall into an endless void. We can do this by creating a simple Square Sprite, and giving it a BoxCollider2D. NOTE: The layer of this object must be set to "Terrain".

![Creating Square](~/images/creatingSquare.PNG)

![Ground Configuration](~/images/groundConfiguration.PNG)

Now we can hit the "Play" button in the Unity Editor to test out our creation:

![Go Into Play Mode](~/images/goIntoPlayMode.PNG)

Except... when we enter play mode, we get an error in the console saying "There is no test player currently in the game"

![Enemy Player Exception](~/images/enemyPlayerException.PNG)

Because our enemy's logic relies on there being a player in the game, it throws an error because we currently don't have a player in the scene to test with. We can fix this by going to WeaverCore -> Insert -> Demo Player, to insert a player into our scene.

![Demo Player](~/images/demoPlayer.PNG)

This demo player is a very basic version of the knight, with only the abilities to move around, double jump, and attack.

**Controls**

| Control Type  | Input |
| ------------- | ------------- |
| Basic Movement | WASD or Arrow Keys  |
| Attack | X Key  |
| Jump and Double Jump | Z Key  |

*Controllers are also supported!*

With the knight now inserted, we can play the game and see our enemy in action!

> [!Video https://www.youtube.com/embed/4y60ZC2FksU]


# Creating bosses

Now, lets convert our new enemy into a boss. Only a few modifications need to be done. First, the CustomEnemy should inherit from [Boss](xref:WeaverCore.Features.Boss) instead of [Enemy](xref:WeaverCore.Features.Enemy). Second, instead of using [IEnemyMove](xref:WeaverCore.Interfaces.IEnemyMove), you should be using [IBossMove](xref:WeaverCore.Interfaces.IBossMove) for each of the moves:

**CustomEnemy.cs**
```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Interfaces;

//--//--//--//--//--//--//--//This class inherits from Boss now
public class CustomEnemy : Boss
//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
{
	//--//--//--//--//--//--//--
    IBossMove[] moves;
	//--//--//--//--//--//--//--

    uint mainRoutineID;

    void Start()
    {
		//--//--//--//--//--//--//--//--//--//--//--//--//
        moves = GetComponents<IBossMove>();
		//--//--//--//--//--//--//--//--//--//--//--//--//
        mainRoutineID = StartBoundRoutine(MainRoutine());
    }

    IEnumerator MainRoutine()
    {
        //Loop forever
        while (true)
        {
            foreach (var move in moves)
            {
                yield return RunMove(move);

                //Wait 0.5 seconds before going to the next move
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
```

**LungeMove.cs**
```cs
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Components;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

//--//--//--//--//--//--//--//--//Move inherits from IBossMove now
public class LungeMove : MonoBehaviour, IBossMove
//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
{
    public bool MoveEnabled => true;

    Rigidbody2D rb;
    GroundDetector groundDetector;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundDetector = GetComponent<GroundDetector>();
        if (groundDetector == null)
        {
            groundDetector = gameObject.AddComponent<GroundDetector>();
        }
    }

    public IEnumerator DoMove()
    {
        var startPos = transform.position;
        var destPos = Player.Player1.transform.position;
        float time = 0.6f;


        //Calculate the initial velocity needed to get from the current position to the player's position in 0.6 seconds
        var lungeVelocity = MathUtilties.CalculateVelocityToReachPoint(startPos, destPos, time);

        //Set the velocity to start the lunge
        rb.velocity = lungeVelocity;

        //Wait for 0.6 seconds
        yield return new WaitForSeconds(time);

        //Wait until the enemy is touching the ground
        yield return new WaitUntil(() => groundDetector.TouchingGround);
    }

    public void OnCancel() { }

    public void OnDeath() { }

	//--//--//New function provided by IBossMove
    public void OnStun() { }
	//--//--//--//--//--//--//--//--//--//--//
}
```

**SlideMove.cs**
```cs
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

//--//--//--//--//--//--//Move inherits from IBossMove now
public class SlideMove : MonoBehaviour, IBossMove
//--//--//--//--//--//--//--//--//--//--//--//--//--//--//
{
    [SerializeField]
    float moveSpeed = 5f;

    [SerializeField]
    float moveDuration = 10;

    public bool MoveEnabled => true;

    Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public IEnumerator DoMove()
    {
        //Run the Move() function every frame until 10 seconds have elapsed
        yield return CoroutineUtilities.RunForPeriod(moveDuration, Move);

        //Stop moving when finished
        rb.velocity = Vector2.zero;
    }

    void Move()
    {
        //If the player is to the right of the enemy
        if (Player.Player1.transform.position.x >= transform.position.x)
        {
            //Set the horizontal velocity to move to the right
            rb.velocity = rb.velocity.With(x: moveSpeed);
        }
        //Otherwise, if the player is to the left of the enemy
        else
        {
            //Set the horizontal velocity to move to the left
            rb.velocity = rb.velocity.With(x: -moveSpeed);
        }
    }

    public void OnCancel()
    {
        //Stop moving if the move is cancelled
        rb.velocity = Vector2.zero;
    }

    public void OnDeath()
    {
        //Stop moving if the enemy dies while this move is running
        rb.velocity = Vector2.zero;
    }

//--//--//--//New function provided by IBossMove
    public void OnStun()
    {
		//Stop moving if the enemy is stunned while this move is running
        rb.velocity = Vector2.zero;
    }
//--//--//--//--//--//--//--//--//--//--//--//
}
```

The [Boss](xref:WeaverCore.Features.Boss) class shares many of the same things the [Enemy](xref:WeaverCore.Features.Enemy) has, but with two key additions:

1. The ability to get stunned
2. Multiple Phases/Stages

Bosses can have stun moments, which occur when their health depletes to a certain point. When a stun occurs, all routines started via [StartBoundRoutine](xref:WeaverCore.Features.Enemy.StartBoundRoutine(System.Collections.IEnumerator)) are stopped, any move that was running is stopped, and the boss's phase counter also gets increased ([BossStage](xref:WeaverCore.Features.Boss.BossStage)).

Stuns can be useful to execute a certain action when a certain health milestone is reached, whether it be giving the player a chance to heal, or to make the boss more difficult.

Lets add a stun to our new boss. If the health of the boss goes below 50%, then the boss will have a shorter delay between moves:

```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Interfaces;

public class CustomEnemy : Boss
{
    IBossMove[] moves;

    uint mainRoutineID;

    float moveDelay = 0.5f;

    void Start()
    {
        moves = GetComponents<IBossMove>();
        mainRoutineID = StartBoundRoutine(MainRoutine());

//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--
        //When the health is depleted to 50%, trigger a stun
        AddStunMilestone(Health.Health / 2);
//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--
    }

    IEnumerator MainRoutine()
    {
        //Loop forever
        while (true)
        {
            foreach (var move in moves)
            {
                yield return RunMove(move);

                //Wait a bit before going to the next move
                yield return new WaitForSeconds(moveDelay);
            }
        }
    }

//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--
    //Called when the boss is stunned
    protected override void OnStun()
    {
        base.OnStun();
        //When the boss is stunned, cut to move delay in half
        moveDelay /= 2f;

		//Since all bound coroutines get stopped when stunned, we need to start the MainRoutine again
        mainRoutineID = StartBoundRoutine(MainRoutine());
    }
//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--
}

```

We will also modify the SlideMove so that when the boss is in its second stage, the boss will slide faster:

```cs
using System.Collections;
using UnityEngine;
using WeaverCore;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

public class SlideMove : MonoBehaviour, IBossMove
{
    [SerializeField]
    float moveSpeed = 5f;

    [SerializeField]
    float moveDuration = 10;

    public bool MoveEnabled => true;

    Rigidbody2D rb;
    Boss boss;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--
        boss = GetComponent<Boss>();
//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--
    }

    public IEnumerator DoMove()
    {
        //Run the Move() function every frame until 10 seconds have elapsed
        yield return CoroutineUtilities.RunForPeriod(moveDuration, Move);

        //Stop moving when finished
        rb.velocity = Vector2.zero;
    }

    void Move()
    {
//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--
        float speed = moveSpeed;
        //Double the slide speed if the boss is on stage 2
        if (boss.BossStage == 2)
        {
            speed *= 2;
        }
//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--

        //If the player is to the right of the enemy
        if (Player.Player1.transform.position.x >= transform.position.x)
        {
            //Set the horizontal velocity to move to the right
            rb.velocity = rb.velocity.With(x: speed);
        }
        //Otherwise, if the player is to the left of the enemy
        else
        {
            //Set the horizontal velocity to move to the left
            rb.velocity = rb.velocity.With(x: -speed);
        }
    }

    public void OnCancel()
    {
        //Stop moving if the move is cancelled
        rb.velocity = Vector2.zero;
    }

    public void OnDeath()
    {
        //Stop moving if the enemy dies while this move is running
        rb.velocity = Vector2.zero;
    }

    public void OnStun()
    {
        //Stop moving if the enemy is stunned while this move is running
        rb.velocity = Vector2.zero;
    }
}

```

Now lets test out the boss. The boss will get harder after half it's health is depleted:

> [!Video https://www.youtube.com/embed/JtN5RMygspk]


# Adding it to the game

Now that our new boss is completed, we now need to figure out how it's going to be added to the game. There are two ways we can do this:

## Method 1 - Replacing an existing Enemy/Boss

The easiest method is to replace an existing boss. To make our new boss replace an existing one, we need to inherit from either [BossReplacement](xref:WeaverCore.Features.BossReplacement) or [EnemyReplacement](xref:WeaverCore.Features.EnemyReplacement)

```cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Features;
using WeaverCore.Interfaces;

//--//--//--//--//--//--//Now inherits from BossReplacement
public class CustomEnemy : BossReplacement
{
    IBossMove[] moves;

    uint mainRoutineID;

    float moveDelay = 0.5f;
	...
}
```

When we head back to the Unity Editor, you will see a new field on the CustomEnemy component. This is where we put the name of the enemy we want to replace.

![Enemy To Replace Field](~/images/EnemyToReplaceField.PNG)

Now we need to find the name of the enemy we want to change. Luckily, WeaverCore provides a way of finding the exact name of an enemy in-game. First, start up the game with the WeaverCore mod installed. Then, go to the area with the enemy/boss you want to change. For this tutorial we will use Marmu. To get the name of Marmu, you will need to pause the game and open the debug tools by pressing CTRL and Numpad 7 on your keyboard (or you can open it via the WeaverCore Settings Menu). It is from here we can find the exact name of Marmu:

![Finding Exact Name](~/images/findingExactName.gif)

> [!NOTE]
> This is just one way of getting the name of an enemy. Another way would be to use Debug  Mod and find the name in the "Enemies" list

In this case, the exact name we want is "Ghost Warrior Marmu", and that is the name we put into this field to replace Marmu with our own custom enemy.

![Marmu Enemy To Replace](~/images/marmuEnemyToReplace.PNG)

### Adding the enemy to the registry

Now we need to drag the enemy object into the "Assets" folder to create a prefab of our enemy:

![Create Enemy Prefab](~/images/createEnemyPrefab.gif)

And finally, add the enemy as a new entry into the mod's registry:

![Add Enemy To Registry](~/images/addEnemyToRegistry.gif)

Now you can build the mod via WeaverCore -> Compilation -> Mod, start up the game, and see your newly created enemy in the game!

> [!Video https://www.youtube.com/embed/5S56_1dUZos]

...Except, there is one slight problem. When the boss dies, we aren't sent back to Godhome. We are stuck in this room forever. This is an easy fix however, we just need to trigger the ending sequence when the boss dies:

```cs
public class CustomEnemy : BossReplacement
{
	...
    //Called when the boss is stunned
    protected override void OnStun()
    {
        base.OnStun();
        //When the boss is stunned, cut to move delay in half
        moveDelay /= 2f;

        //Since all bound coroutines get stopped when stunned, we need to start the MainRoutine again
        mainRoutineID = StartBoundRoutine(MainRoutine());
    }

//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--
    //Called when the boss dies
    protected override void OnDeath()
    {
        base.OnDeath();

        //After two seconds, end the boss battle and return to godhome
        Boss.EndBossBattle(2f);
    }
//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--//--
}
```

Now, when the boss dies, the ending sequence will get triggered after 2 seconds:

> [!Video https://www.youtube.com/embed/CLIfHvfdrf4]


## Method 2 - Manually Instantiating it (Advanced)

The other option is to instantiate it manually via GameObject.Instantiate or something similar. To do this, first create a prefab of the enemy and add it to the registry (just like in Method 1).

Then, to load the prefab from the registry at any time, you can call [Registry.GetFeatures](xref:WeaverCore.Registry.GetFeatures``1) to load the enemy prefab. Then you can use GameObject.Instantiate to create the enemy in-game:

```cs
public void LoadEnemy()
{
	//Load the prefab
    var enemyPrefab = Registry.GetFeature<CustomEnemy>();

    //Instantiate the enemy
    var instance = GameObject.Instantiate(enemyPrefab);
}
```
