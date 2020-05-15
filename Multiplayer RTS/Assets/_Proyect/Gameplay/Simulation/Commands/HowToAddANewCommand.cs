/*
 
 
creating new command types:

1- CommandUtils - implement the de/serialization and the command is valid function. 

2- NetworkEventProcessorSystem - Update: CommandCallback <- update the function|
                                 and the call of"CommandStorageSystem.QueueNetworkedCommands" 
                                 and the event data object passed in -> (3)
3- VolatileCommandSystem - update SendCommandToNetworkFunction <- update the content object
4- CommandStorageSystem - Add two static dictionaries
                          Update the clear state function.
                          Update: GetAllVolatileCommandsSerialized <- add and element containing an array of the commands of that type to be executed
                          that turn.
                          Must add a new override of the "TryAddLocalCommand" function
                          Must update the "QueueVolatileCommands" function.
                          Must update "QueueNetworkedCommands" <- add an argument for a array of the new command type
                          update the "AreVolatileCommands" function.

6- CommandExecutionSystem - Update: add the execution part the new command on the Update
                            Add a ExecuteCommand Variation with the new command as an argument.

 
                            
 











 
 
 */