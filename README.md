# Aristurtle MonoGame Save
This is an implementation of a save file system that can be used with MonoGame to save game save data in a way that is less likely to be opened and edited by the average user.

There is no 100% sure way of stopping people from editing a save file, but the goal is to make it annoying enough that the average users won't bother with it.

## How It Works
There are two methods current for creating a save file

### SaveFileWriter.ToPng
The `SaveFileWriter.ToPng` method will create a PNG file and embed the save data within a custom SAVE chunk in the png file.  You can take a screenshot of the current game screen by getting graphics device backbuffer info and using that as the pixels for the png or you can supply your own Texture2D.

Since the data is written to a PNG chunk, a checksum value is created and used as validation when loading the data back in to ensure integrity of data.

To load the data, just use the `SaveFileReader.FromPng` to load that same png back in

### SaveFileWRiter.ToBin
The `SaveFileWRiter.ToBin` method will create a binary encoded file that contains the save data.  The binary file created has a unique signature header appeneded that to signify it's ment to be read by this library, followed by the data len, the data chunk, then finally the checksum of the data.  When reading the data back in, the checksum is used to validate data integrity

To load the data, just use the `SaveFileReader.FromBin` method.

## Examples
Examples of both the PNG method and the Bin method can be found in the [/examples](Examples) directory.

## License
This is licensed under the MIT license.  You can find the full license text in the [LICENSE](LICENSE) file.