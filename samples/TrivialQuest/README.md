# TrivialQuest
This sample demonstrates the use of Events and Quests in the Play Games Plugin 
for Unity.  You should **follow the normal setup instructions for other samples**,
but in addition you will need to perform the following steps to get the full 
sample functionality:

  1. Go to the Google Play Developer Console and open your game project.
  1. Create four **Events** named `red`, `green`, `blue`, and `yellow`.  
  Each event represents attacking a monster of the given color.
  1. Replace the variables `EVENT_ATTACK_RED`, `EVENT_ATTACK_GREEN`, 
  `EVENT_ATTACK_BLUE`, and  `EVENT_ATTACK_YELLOW` with the IDs of the events 
  that you created.
  1. Create at least one **Quest** based on achieving some of the events that 
  you just created, make sure the Quest start date is in the past and the 
  Quest end date is in the future.  For example, you could create a Quest with 
  the following metadata:
  	
  		Name: Attack 5 Reds
  		Description: Attack a red monster at least 5 times.
  		Completion Criteria: 'red' is increased by 5.
  		Start Date: (today's date)
  		End Date: (a week from now)
  		
  Once you have completed all of these steps, open the game and sign in.  
  You should be able to see the quests you have created, complete them, 
  and claim the rewards.
