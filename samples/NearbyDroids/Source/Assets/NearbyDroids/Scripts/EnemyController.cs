// <copyright file="EnemyController.cs" company="Google Inc.">
// Copyright (C) 2015 Google Inc.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>

namespace NearbyDroids
{
    using UnityEngine;

    /// <summary>
    /// Enemy controller.
    /// Heavily inspired by
    /// http://unity3d.com/learn/tutorials/projects/2d-roguelike
    /// </summary>
    public class EnemyController : MovingObject
    {
        // The amount of points to subtract from the player when attacking.
        public int playerDamage;

        // Variable of type Animator to store a reference to the enemy's
        // Animator component.
        private Animator animator;

        // Boolean to determine whether or not enemy should skip a turn or
        // move this turn.
        private bool skipMove;

        // Start overrides the virtual Start function of the base class.
        protected override void Start()
        {
            // Register this enemy with our instance of GameManager by adding
            // it to a list of Enemy objects.
            // This allows the GameManager to issue movement commands.
            GameManager.Instance.AddEnemyToList(this);

            // Get and store a reference to the attached Animator component.
            animator = GetComponent<Animator>();

            // Call the start function of our base class MovingObject.
            base.Start();
        }

        internal void Update()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            if (animator != null)
            {
                if (animator.GetCurrentAnimatorStateInfo(0).IsName("Dead"))
                {
                    Debug.Log("animator state is dead!");
                    // turn off this script since this enemy is dead.
                    enabled = false;
                }
            }
        }

        /// <summary>
        /// Attempts the move.
        /// Override the AttemptMove function of MovingObject to include
        /// functionality needed for Enemy to skip turns.
        /// See comments in MovingObject for more on how base AttemptMove
        /// function works.
        /// </summary>
        /// <param name="xDir">X direction in cell units</param>
        /// <param name="yDir">Y direction</param>
        /// <typeparam name="T">The type of object expected to be hit.</typeparam>
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            // Check if skipMove is true, if so set it to false and skip
            // this turn.
            if (skipMove)
            {
                skipMove = false;
                return;
            }

            // Call the AttemptMove function from MovingObject.
            base.AttemptMove<T>(xDir, yDir);

            // Now that Enemy has moved, set skipMove to true to skip next move.
            skipMove = true;
        }

        /// <summary>
        /// Moves the enemy.
        /// MoveEnemy is called by the GameManger each turn to tell each Enemy
        /// to try to move towards the player.
        /// </summary>
        public void MoveEnemy()
        {
            // Declare variables for X and Y axis move directions,
            // these range from -1 to 1.
            // These values allow us to choose between the cardinal
            // directions: up, down, left and right.
            int xDir = 0;
            int yDir = 0;

            // Find the Player GameObject using it's tag and store a reference
            // to its transform component.
            // This could be a place for some optimization so the players could
            // be cached.  Special attention is needed for when new players show up...
            GameObject[] targets = GameObject.FindGameObjectsWithTag("Player");
            Transform target = null;
            float minDistMagnitude = float.MaxValue;

            foreach (GameObject targ in targets)
            {
                PlayerController player = targ.GetComponent<PlayerController>();
                if (targ.activeSelf && player.Player.Score >= 0)
                {
                    float d = (transform.position - targ.transform.position).sqrMagnitude;
                    if (target == null || d < minDistMagnitude)
                    {
                        target = targ.transform;
                        minDistMagnitude = d;
                    }
                }
            }

            if (target == null)
            {
                return;
            }

            // If the difference in positions is approximately zero
            // (Epsilon) do the following:
            float diff = Mathf.Abs(target.position.x - transform.position.x);
            if (diff < float.Epsilon)
            {
                // If the y coordinate of the target's (player) position
                // is greater than the y coordinate of this enemy's position
                // set y direction 1 (to move up). If not, set it
                // to -1 (to move down).
                yDir = target.position.y > transform.position.y ? 1 : -1;
            }
            else
            {
                // If the difference in positions is not approximately
                // zero (Epsilon) do the following:
                // Check if target x position is greater than enemy's x
                // position, if so set x direction to 1 (move right),
                // if not set to -1 (move left).
                xDir = target.position.x > transform.position.x ? 1 : -1;
            }

            // Call the AttemptMove function and pass in the generic
            // parameter Player,
            // because Enemy is moving and expecting to potentially
            // encounter a Player
            // We can also hit another enemy - so check for MovingObject
            AttemptMove<MovingObject>(xDir, yDir);
        }

        /// <summary>
        /// Raises the cant move event.
        /// OnCantMove is called if Enemy attempts to move into a space
        /// occupied by a Player, it overrides the OnCantMove function
        /// of MovingObject
        /// and takes a generic parameter T which we use to pass in the
        /// component we expect to encounter, in this case Player
        /// </summary>
        /// <param name="component">the component hit.</param>
        /// <typeparam name="T">The type of the component.</typeparam>
        protected override void OnCantMove<T>(T component)
        {
            if (component.gameObject.tag == "enemy")
            {
                Explode();
            }
            else
            {
                // Declare hitPlayer and set it to equal the
                // encountered component.
                PlayerController hitPlayer = component as PlayerController;

                // Call the LoseFood function of hitPlayer passing it
                // playerDamage, the amount of foodpoints to be subtracted.
                hitPlayer.LoseHealth(playerDamage);

                // Set the attack trigger of animator to trigger
                // Enemy attack animation.
                // TODO(wilkinsonclay) - make the animation
                //  animator.SetTrigger("enemyAttack");
            }
        }

        // OnTriggerEnter2D is sent when another object enters a
        // trigger collider attached to this object (2D physics only).
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if the tag of the trigger collided with is Exit.
            if (other.tag == "deadly" || other.tag == "Player")
            {
                animator.Play("enemy_die");
            }
        }

        public void Explode()
        {
            animator.SetTrigger("explode");
        }
    }
}
