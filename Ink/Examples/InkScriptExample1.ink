// Demo Script
// 2020-04-04
// This is a random test example script!

// Variables
#IMPORT int nextNode
VAR nextNode = 0

#IMPORT int health
VAR health = 100

#IMPORT string playrtName
VAR playerName = "Bob"

-> EncounterFountain

=== EncounterFountain
The stench of fish wafts through the streets, and fills the wanderers nostrils.
In the centre of town sits a proud fountain, tossing droplets of confetti from its spout with an air of great importance, while shimmering bubbles ascend from the basin and twirl through the air.

The wanderers approach the fountain, entranced by the dancing water and lured by its trickling whispers.

As they reach the fountain, they spy the glistening of gold coins, collected under the water.
*[Toss a Coin.]
-> Toss
*[Steal the Coins.]
-> Steal


=== Toss

->END


=== Steal

->END