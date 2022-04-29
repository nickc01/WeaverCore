using System.Collections;
using UnityEngine;
using WeaverCore.Enums;

namespace WeaverCore.Assets.Components
{

    /// <summary>
    /// Used on the Demo Player to allow the player to attack
    /// </summary>
    public class PlayerWeaponizer : MonoBehaviour
    {
		EnemyDamager attacker;
		[SerializeField]
		GameObject SlashBox;

		[SerializeField]
		float AttackTime = 0.3f;


		void Awake()
		{
			attacker = SlashBox.GetComponent<EnemyDamager>();
		}

		bool attacking = false;

		// Update is called once per frame
		void Update()
		{
			if (!attacking && PlayerInput.attack.WasPressed)
			{
				attacking = true;
				StartCoroutine(AttackRoutine());
			}
		}

		IEnumerator AttackRoutine()
		{
			var oldRotation = SlashBox.transform.eulerAngles;
			var oldPosition = SlashBox.transform.localPosition;

            if (PlayerInput.up.IsPressed)
            {
				attacker.hitDirection = CardinalDirection.Up;
				//SlashBox.transform.eulerAngles = new Vector3(0, 0, -90f);
				//SlashBox.transform.localPosition = new Vector3(oldPosition.y, oldPosition.x, oldPosition.z);
				SlashBox.transform.RotateAround(transform.position, transform.forward, -90f * Player.Player1.transform.GetScaleX());
			}
            else if (PlayerInput.down.IsPressed)
            {
				attacker.hitDirection = CardinalDirection.Down;
				//SlashBox.transform.eulerAngles = new Vector3(0, 0, 90f);
				SlashBox.transform.RotateAround(transform.position, transform.forward, 90f * Player.Player1.transform.GetScaleX());
				//SlashBox.transform.localPosition = new Vector3(oldPosition.y, oldPosition.x, oldPosition.z);
			}
			else
            {
				if (transform.localScale.x > 0)
				{
					attacker.hitDirection = CardinalDirection.Left;
				}
				else
                {
					attacker.hitDirection = CardinalDirection.Right;
				}
				//SlashBox.transform.eulerAngles = new Vector3(0, 0, 0f);
				//SlashBox.transform.localPosition = new Vector3(oldPosition.x, oldPosition.y, oldPosition.z);
			}
			SlashBox.SetActive(true);
			yield return new WaitForSeconds(AttackTime);
			SlashBox.SetActive(false);
			SlashBox.transform.eulerAngles = oldRotation;
			SlashBox.transform.localPosition = oldPosition;
			attacking = false;
		}
	}
}
