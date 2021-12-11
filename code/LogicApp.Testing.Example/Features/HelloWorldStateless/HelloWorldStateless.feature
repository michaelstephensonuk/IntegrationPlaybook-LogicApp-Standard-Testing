Feature: HelloWorldStateless
	Simple test of a basic stateless logic app

#Important Note:
#For stateless logic apps be sure to set the below app setting
#Workflows.{workflow-name}.OperationOptions = WithStatelessRunHistory

@CleanTheSystem
Scenario: Green Path
	Given I have a request to send to the logic app
	And the logic app test manager is setup
	When I send the message to the logic app
	Then the logic app will start running
	And the logic app will receive the message
	And the logic app will send a reply
	And the logic app will complete successfully