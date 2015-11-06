# README

**Grupo 7**

* `ist170315` - Rodrigo Amaral Cabral Vieira
* `ist170958` - José Gonçalo Simões Rodrigues
* `ist173557` - José Pedro de Almeida Arvela

## Running test script

All test scripts and network configurations are located in PuppetMaster\tests

To run the script from the application simply type:

"RunScript ..\..\tests\script.txt"

## Formatting

To maintain code coherence please verify the following settings on Visual Studio

* **tools\options\TextEditor\C#\Formating\NewLines**  
  uncheck all boxes except those under **New line options for expresions**.

## Extra commands

* `Import <path>`  
  Imports a network configuration file from a specified file (must be a .txt file)
* `RunScript <path>`  
  Runs a PuppetMaster script file from a specified file (must be a .txt file)
* `StartNetwork`  
  Spawn the all of the node processes  
  It is important to note that when running the commands to define the network topology, the
  processes are not started. This is the command that starts the network.
* `AddTopic <targetBroker> <interestedNode> <topic>`  
  Used for testing the broker's filtering. Fakes a subscription in the target broker.
* `SpawnPublication <broker> <topic> <sequenceNumber>`  
  Used for testing the broker's filtering. Creates a test publication at the specified broker with
  the specified topic and sequence number.
