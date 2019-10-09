# multiAgents.py
# --------------
# Licensing Information:  You are free to use or extend these projects for
# educational purposes provided that (1) you do not distribute or publish
# solutions, (2) you retain this notice, and (3) you provide clear
# attribution to UC Berkeley, including a link to http://ai.berkeley.edu.
# 
# Attribution Information: The Pacman AI projects were developed at UC Berkeley.
# The core projects and autograders were primarily created by John DeNero
# (denero@cs.berkeley.edu) and Dan Klein (klein@cs.berkeley.edu).
# Student side autograding was added by Brad Miller, Nick Hay, and
# Pieter Abbeel (pabbeel@cs.berkeley.edu).


from util import manhattanDistance
from game import Directions
import random, util

from game import Agent

class ReflexAgent(Agent):
    """
      A reflex agent chooses an action at each choice point by examining
      its alternatives via a state evaluation function.

      The code below is provided as a guide.  You are welcome to change
      it in any way you see fit, so long as you don't touch our method
      headers.
    """


    def getAction(self, gameState):
        """
        You do not need to change this method, but you're welcome to.

        getAction chooses among the best options according to the evaluation function.

        Just like in the previous project, getAction takes a GameState and returns
        some Directions.X for some X in the set {North, South, West, East, Stop}
        """
        # Collect legal moves and successor states
        legalMoves = gameState.getLegalActions()

        # Choose one of the best actions
        scores = [self.evaluationFunction(gameState, action) for action in legalMoves]
        bestScore = max(scores)
        bestIndices = [index for index in range(len(scores)) if scores[index] == bestScore]
        chosenIndex = random.choice(bestIndices) # Pick randomly among the best

        "Add more of your code here if you want to"

        return legalMoves[chosenIndex]

    def evaluationFunction(self, currentGameState, action):
        """
        Design a better evaluation function here.

        The evaluation function takes in the current and proposed successor
        GameStates (pacman.py) and returns a number, where higher numbers are better.

        The code below extracts some useful information from the state, like the
        remaining food (newFood) and Pacman position after moving (newPos).
        newScaredTimes holds the number of moves that each ghost will remain
        scared because of Pacman having eaten a power pellet.

        Print out these variables to see what you're getting, then combine them
        to create a masterful evaluation function.
        """
        # Useful information you can extract from a GameState (pacman.py)
        successorGameState = currentGameState.generatePacmanSuccessor(action)
        newPos = successorGameState.getPacmanPosition()
        newFood = successorGameState.getFood()
        newGhostStates = successorGameState.getGhostStates()
        newScaredTimes = [ghostState.scaredTimer for ghostState in newGhostStates]

        "*** YOUR CODE HERE ***"
        return successorGameState.getScore()

def scoreEvaluationFunction(currentGameState):
    """
      This default evaluation function just returns the score of the state.
      The score is the same one displayed in the Pacman GUI.

      This evaluation function is meant for use with adversarial search agents
      (not reflex agents).
    """
    return currentGameState.getScore()

class MultiAgentSearchAgent(Agent):
    """
      This class provides some common elements to all of your
      multi-agent searchers.  Any methods defined here will be available
      to the MinimaxPacmanAgent, AlphaBetaPacmanAgent & ExpectimaxPacmanAgent.

      You *do not* need to make any changes here, but you can if you want to
      add functionality to all your adversarial search agents.  Please do not
      remove anything, however.

      Note: this is an abstract class: one that should not be instantiated.  It's
      only partially specified, and designed to be extended.  Agent (game.py)
      is another abstract class.
    """

    def __init__(self, evalFn = 'scoreEvaluationFunction', depth = '2'):
        self.index = 0 # Pacman is always agent index 0
        self.evaluationFunction = util.lookup(evalFn, globals())
        self.depth = int(depth)

class MinimaxAgent(MultiAgentSearchAgent):
    """
      Your minimax agent (question 2)
    """
    # Implementation of a level of the minimax agent.
    # state is the state at this action
    # index is the index of the agent currently acting. Pacman is 0, the ghosts are > 0
    # cdepth is the current depth, where each level contains [index] layers.
    def minimax(self, state, index, cdepth):
        # Increment the iteration
        if index == self.index:
            cdepth = cdepth + 1
        # if we have looped back around to the acting agent and reached the specified max depth, terminate
        if cdepth == self.depth:
            return self.evaluationFunction(state)

        actions = state.getLegalActions(index)
        if len(actions) == 0:
            return self.evaluationFunction(state)
        scores = map(lambda action: self.minimax(state.generateSuccessor(index, action), (index + 1) % state.getNumAgents(), cdepth), actions)
        if index == 0:
            return max(scores)
        return min(scores)
    # It seems like the program is only designed to play with Pacman, but this function is built to
    # allow the ghosts to play as well
    def getAction(self, gameState):
        """
          Returns the minimax action from the current gameState using self.depth
          and self.evaluationFunction.
        """
        "*** YOUR CODE HERE ***"
        actions = gameState.getLegalActions(self.index)
        scores = map(lambda action: self.minimax(gameState.generateSuccessor(self.index, action), (self.index + 1) % gameState.getNumAgents(), 0), actions)
        index = 0
        if self.index == 0:
            #print("max " + str(max(scores)) + ", " + str(self.index))
            index = scores.index(max(scores))
        else:
            #print("min " + str(min(scores)) + ", " + str(self.index))
            index = scores.index(min(scores))
        return actions[index]

class AlphaBetaAgent(MultiAgentSearchAgent):
    """
      Your minimax agent with alpha-beta pruning (question 3)
    """
    # Iteration of the alphabeta function
    # state is the state before acting
    # index is the current index of the acting agent
    # cdepth is the current depth, where each level contains [index] layers.
    # alpha is the best known path in the branch leading out to this node for the maximizer
    # beta is the best known path in the branch leading out to this node for the minimizer
    def alphabeta(self, state, index, cdepth, alpha, beta):
        if index == self.index:
            cdepth = cdepth + 1
        if cdepth == self.depth:
            return self.evaluationFunction(state)

        actions = state.getLegalActions(index)
        if len(actions) == 0:
            return self.evaluationFunction(state)
        if index == 0:
            value = -float("inf")
            for action in actions:
                value = max(value, self.alphabeta(state.generateSuccessor(index, action), (index + 1) % state.getNumAgents(), cdepth, alpha, beta))
                if value > beta:
                    return value
                alpha = max(alpha, value)
            return value
        value = float("inf")
        for action in actions:
            value = min(value, self.alphabeta(state.generateSuccessor(index, action), (index + 1) % state.getNumAgents(), cdepth, alpha, beta))
            if value < alpha:
                return value
            beta = min(beta, value)
        return value

    # This is not built to allow the agent to be the ghost, but it is fairly easy to expand it,
    # just add a different case for the minimizers.
    def getAction(self, gameState):
        """
          Returns the minimax action using self.depth and self.evaluationFunction
        """
        aindex = -1
        actions = gameState.getLegalActions(self.index)
        alpha = -float("inf")
        beta = float("inf")
        value = -float("inf")
        for i in range(0, len(actions)):
            action = actions[i]
            nextval = self.alphabeta(gameState.generateSuccessor(self.index, action), (self.index + 1) % gameState.getNumAgents(), 0, alpha, beta)
            if nextval > value:
                value = nextval
                aindex = i
            alpha = max(alpha, value)

        return actions[aindex]
class ExpectimaxAgent(MultiAgentSearchAgent):
    """
      Your expectimax agent (question 4)
    """

    def getAction(self, gameState):
        """
          Returns the expectimax action using self.depth and self.evaluationFunction

          All ghosts should be modeled as choosing uniformly at random from their
          legal moves.
        """
        "*** YOUR CODE HERE ***"
        util.raiseNotDefined()

def betterEvaluationFunction(currentGameState):
    """
      Your extreme ghost-hunting, pellet-nabbing, food-gobbling, unstoppable
      evaluation function (question 5).

      DESCRIPTION: <write something here so we know what you did>
    """
    "*** YOUR CODE HERE ***"
    util.raiseNotDefined()

# Abbreviation
better = betterEvaluationFunction

