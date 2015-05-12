// <copyright file="PlayerController.cs" company="Google Inc.">
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
    /// Player controller.
    /// Heavily inspired by http://unity3d.com/learn/tutorials/projects/2d-roguelike
    /// </summary>
    public class PlayerController : MovingObject
    {
        // Delay time in seconds to restart level.
        public float restartLevelDelay = 1f;

        // Number of points to add to player  points
        public int pointsPerCoin = 10;
        public int pointsPerGem = 20;

        private Animator animator;

        // nearby connections player info.
        private PlayerInfo player;

        public PlayerInfo Player
        {
            get
            {
                return player;
            }

            set
            {
                player = value;
            }
        }

        // Start overrides the Start function of MovingObject
        protected override void Start()
        {
            // Get a component reference to the Player's animator component
            animator = GetComponent<Animator>();

            // Call the Start function of the MovingObject base class.
            base.Start();
        }

        private void Update()
        {
            // If it's not the player's turn, exit the function.
            if (Player.Moved || Player.Score < 0)
            {
                return;
            }

            if (player == null)
            {
                Debug.Log("Player is null?");
                return;
            }

            // if this is a remote player, don't move.
            if (!player.IsLocal)
            {
                return;
            }

            int horizontal = 0;
            int vertical = 0;

            // Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
            horizontal = (int)Input.GetAxisRaw("Horizontal");

            // Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
            vertical = (int)Input.GetAxisRaw("Vertical");

            // Check if moving horizontally, if so set vertical to zero.
            if (horizontal != 0)
            {
                vertical = 0;
            }

            // look for touches
            if (horizontal == 0 && Input.touchCount > 0)
            {
                // Store the first touch detected.
                Touch t = Input.touches[0];

                // move towards the touch
                if (t.phase == TouchPhase.Stationary ||
                    t.phase == TouchPhase.Moved)
                {
                    // If the finger is on the screen,
                    // move the object smoothly to the touch position
                    Vector3 screenPos = new Vector3(t.position.x, t.position.y, 10);
                    Vector3 touchPosition = Camera.main.ScreenToWorldPoint(screenPos);

                    // compare to players position.
                    float x = touchPosition.x - transform.position.x;
                    float y = touchPosition.y - transform.position.y;

                    // Check if the difference along the x axis is
                    // greater than the difference along the y axis.
                    if (Mathf.Abs(x) > Mathf.Abs(y))
                    {
                        horizontal = x > 0 ? 1 : -1;
                    }
                    else
                    {
                        vertical = y > 0 ? 1 : -1;
                    }
                }
            }
 
            // Check if we have a non-zero value for horizontal or vertical
            if (horizontal != 0 || vertical != 0)
            {
                // Call AttemptMove passing in the generic parameter Wall,
                // since that is what Player may interact with if they
                // encounter one (by attacking it)
                // Pass in horizontal and vertical as parameters to specify
                // the direction to move Player in.
                AttemptMove<Zapper>(horizontal, vertical);
            }
        }

        // AttemptMove overrides the AttemptMove function in the base class 
        // MovingObject
        // AttemptMove takes a generic parameter T which for Player will be of
        // the type Wall, it also takes integers for x and y direction to move in.
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            // Call the AttemptMove method of the base class, passing in the
            // component T (in this case Wall) and x and y direction to move.
            base.AttemptMove<T>(xDir, yDir);

            // Hit allows us to reference the result of the Linecast done in Move.
            RaycastHit2D hit;

            // If Move returns true, meaning Player was able to move into an
            // empty space.
            if (Move(xDir, yDir, out hit))
            {
                // TODO: play a sound or something.
            }

            // Since the player has moved and lost food points,
            // check if the game has ended.
            CheckIfDead();

            // Let the manager the player has completed the turn.
            Player.Moved = true;
        }

        // OnCantMove overrides the abstract function OnCantMove in MovingObject.
        // It takes a generic parameter T which in the case of Player
        // is a Wall which the player can attack and destroy.
        protected override void OnCantMove<T>(T component)
        {
            // TODO: handle walking into the zapper
            // Zapper hit = component as Zapper;
            //  animator.SetTrigger ("playerBuzz");
        }

        // OnTriggerEnter2D is sent when another object enters a trigger
        // collider attached to this object (2D physics only).
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if the tag of the trigger collided with is Exit.
            if (other.tag == "exit")
            {
                // Invoke the Restart function to start the next level with 
                // a delay of restartLevelDelay (default 1 second).
                Invoke("Restart", restartLevelDelay);

                // Disable the player object since level is over.
                enabled = false;
            }
            else if (other.tag == "powerup")
            {
                // Check if the tag of the trigger collided with is Food.
                // Add pointsPerFood to the players current total.
                PlayerInfo.AddScore(player.DeviceId, pointsPerCoin);
                GameManager.Instance.OnScoreChanged(player.DeviceId);

                // Disable the food object the player collided with.
                other.gameObject.SetActive(false);
                GameManager.Instance.OnObjectChanged(
                    other.gameObject.GetComponent<Shareable>());
            }
            else if (other.tag == "gem")
            {
                // Check if the tag of the trigger collided with is a gem.
                // Add pointsPerSoda to players food points total
                PlayerInfo.AddScore(player.DeviceId, pointsPerGem);
                GameManager.Instance.OnScoreChanged(player.DeviceId);

                // Disable the object the player collided with.
                other.gameObject.SetActive(false);
                GameManager.Instance.OnObjectChanged(
                    other.gameObject.GetComponent<Shareable>());
            }
            else if (other.tag == "deadly")
            {
                // Check if the tag of the trigger collided with is Exit.
                LoseHealth(20);
            }
        }

        // Restart reloads the scene when called.
        private void Restart()
        {
            // Load the last scene loaded, in this case Main, the only scene
            // in the game.
            GameManager.Instance.StartNextLevel();
        }

        // LoseFood is called when an enemy attacks the player.
        // It takes a parameter loss which specifies how many points to lose.
        public void LoseHealth(int loss)
        {
            // Set the trigger for the player animator to transition to the 
            // playerHit animation.
            // TODO: play an animation or sound when hit!

            if (Player.Score >= 0)
            {
                // Subtract lost food points from the players total.
                PlayerInfo.AddScore(player.DeviceId, -loss);
                GameManager.Instance.OnScoreChanged(player.DeviceId);
            }

            // Check to see if game has ended.
            CheckIfDead();
        }

        // CheckIfDead checks if the player is out of food points and if so,
        // ends the game.
        private bool CheckIfDead()
        {
             // Check if point total is less than or equal to zero.
            if (Player.Score < 0)
            {
                // Call the GameOver function of GameManager.
                if (Player.IsLocal)
                {
                    GameManager.Instance.GameOver("You were zapped");
                }
            }

            return Player.Score < 0;
        }
    }
}