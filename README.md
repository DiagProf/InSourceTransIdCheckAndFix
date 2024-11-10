# InSourceTransIdCheckAndFix

## Project Overview

**InSourceTransIdCheckAndFix** is a proof-of-concept (POC) project exploring the feasibility of using Roslyn Analyzers and CodeFixes to automatically mark developer strings in code with a unique translation ID. This system is designed to streamline the translation process by ensuring that developer text strings are both appropriately marked and of high quality for translation.

Since translation IDs are assigned by an external tool, this tool (not part of this POC) also integrates a quality check for developer texts. Low-quality developer strings can lead to issues during translation.

## How It Works

The **InSourceTransIdCheckAndFix** operates with the `DevTextToKeyMapperDb.db`, a LiteDB database that manages two collections:

- **NewDeveloperText**: Stores new text strings submitted by developers. These entries are pending review.
- **DevTextToKeyMapper**: Contains all approved developer texts with their associated translation IDs. Only high-quality texts are stored here.

This database (`DevTextToKeyMapperDb.db`) can be shared with a translation team. The team can review entries in the **NewDeveloperText** collection, assess their quality, and, if approved, move them into the **DevTextToKeyMapper** collection. After review, the translation team can use `DevTextToKeyMapperDb.db` to generate a final translation database, `RtTranslation.db`, which the application will use directly.

## Configuration

### InSourceTransIdCheckAndFixConfig.json

The configuration file, `InSourceTransIdCheckAndFixConfig.json`, must specify the absolute path to `DevTextToKeyMapperDb.db` for the Analyzer and CodeFix to function correctly. Additionally, this file allows customization of:
- The name of the generated Translation class.
- The names of factory methods used within the project.

## Future Considerations

This project provides a foundational approach to automating developer text translation in code and ensuring text quality from the start. Further enhancements may include:
- Relative path support for the database.
- Advanced integration for streamlined review and translation ID assignment.

## Usage

1. Configure the path to `DevTextToKeyMapperDb.db` in `InSourceTransIdCheckAndFixConfig.json`.
2. Set up the Translation class and factory method names as desired.
3. Run the analyzer and CodeFix to automatically mark developer strings with translation IDs and flag any text that requires review.
4. Share `DevTextToKeyMapperDb.db` with the translation team for quality checking and ID assignment.

This POC offers a practical solution for marking, reviewing, and managing developer-facing strings in code, ensuring a smoother translation workflow.
