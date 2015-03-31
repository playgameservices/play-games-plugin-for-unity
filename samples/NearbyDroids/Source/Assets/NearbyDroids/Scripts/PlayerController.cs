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

        // nearby connections device id.  it is stable
        // across games and device restarts, so we use it as the key uniquely identifying the player.
        private string deviceId = string.Empty;

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
            if (!GameManager.Instance.PlayersTurn)
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
 
            // Check if we have a non-zero value for horizontal or vertical
            if (horizontal != 0 || vertical != 0)
            {
                // Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
                // Pass in horizontal and vertical as parameters to specify the direction to move Player in.
                AttemptMove<Zapper>(horizontal, vertical);
            }
        }

        // AttemptMove overrides the AttemptMove function in the base class MovingObject
        // AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            // Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
            base.AttemptMove<T>(xDir, yDir);

            // Hit allows us to reference the result of the Linecast done in Move.
            RaycastHit2D hit;

            // If Move returns true, meaning Player was able to move into an empty space.
            if (Move(xDir, yDir, out hit))
            {
                // Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
            }

            // Since the player has moved and lost food points, check if the game has ended.
            CheckIfDead();

            // Set the playersTurn boolean of GameManager to false now that players turn is over.
            GameManager.Instance.PlayersTurn = false;
        }

        // OnCantMove overrides the abstract function OnCantMove in MovingObject.
        // It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
        protected override void OnCantMove<T>(T component)
        {
            // TODO: handle walking into the zapper
            // Zapper hit = component as Zapper;
            //  animator.SetTrigger ("playerBuzz");
        }

        // OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if the tag of the trigger collided with is Exit.
            if (other.tag == "exit")
            {
                // Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
                Invoke("Restart", restartLevelDelay);

                // Disable the player object since level is over.
                enabled = false;
            }
            else if (other.tag == "powerup")
            {
                // Check if the tag of the trigger collided with is Food.
                // Add pointsPerFood to the players current total.
                PlayerInfo.AddScore(deviceId, pointsPerCoin);

                // Disable the food object the player collided with.
                other.gameObject.SetActive(false);
            }
            else if (other.tag == "gem")
            {
                // Check if the tag of the trigger collided with is a gem.
                // Add pointsPerSoda to players food points total
                PlayerInfo.AddScore(deviceId, pointsPerGem);

                // Disable the object the player collided with.
                other.gameObject.SetActive(false);
            }
        }

        // Restart reloads the scene when called.
        private void Restart()
        {
            // Load the last scene loaded, in this case Main, the only scene in the game.
            Application.LoadLevel(Application.loadedLevel);
        }

        // LoseFood is called when an enemy attacks the player.
        // It takes a parameter loss which specifies how many points to lose.
        public void LoseHealth(int loss)
        {
            // Set the trigger for the player animator to transition to the playerHit animation.
            // animator.SetTrigger ("playerHit");

            // Subtract lost food points from the players total.
            PlayerInfo.AddScore(deviceId, -loss);

            // Check to see if game has ended.
            CheckIfDead();
        }

        // CheckIfDead checks if the player is out of food points and if so, ends the game.
        private void CheckIfDead()
        {
            // Check if food point total is less than or equal to zero.
            if (PlayerInfo.GetScore(deviceId) <= 0)
            {
                // Call the GameOver function of GameManager.
                GameManager.Instance.GameOver();
            }
        }
    }
}