# AD Topology Diagram Tool

## Description
The AD Topology Diagram Tool is designed to create detailed Active Directory topology diagrams. This includes domain diagrams featuring all domains, servers, realms, and trusts, as well as replication topology diagrams covering sites, site links, servers, and connection objects. Servers are color-coded by domain for clarity. The tool operates without installation; simply extract the zip file, and it's ready to run.

## Features
- **Domain Diagram Generation**: Visualize all domains, servers, realms, and trusts.
- **Replication Topology Diagram**: Diagram of replication topology covering sites, site links, and servers.
- **No Installation Required**: Just extract and run.

## Components
1. **Diagramming Tool**: Utilizes input from the configuration to display the UI and manage the execution of data readers based on the selected diagram type.
2. **Data Readers**:
   - **Domain Data Reader**: Gathers data for domain diagrams.
   - **Site Data Reader**: Collects data for site replication diagrams.
3. **Diagram Generation Library**:
   - **Arranging**: Designs an organized layout for diagrams.
   - **Generation**: Generates diagrams in Visio format (OpenXml).

## Installation
1. Download the latest release from the [Releases](https://github.com/goran-mihelcic/ADTD.Net/releases) tab.
2. Extract the zip file to your desired location.
3. Run the executable to start the tool.

## Usage
To start the tool, execute the following steps:
1. Open the tool from the extracted folder.
2. Choose the type of diagram you want to generate.
3. Configure the necessary parameters (if any).
4. Click on `Generate` to create the diagram.

## Contributing
We welcome contributions from the community! Here are ways you can contribute:
- **Reporting Bugs**
- **Suggesting Enhancements**
- **Pull Requests**

Please read our [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## License
This project is licensed under the [MIT License](LICENSE.md).

## Acknowledgments
- Thanks to all the contributors who’ve helped to build this tool.

## Contact
For help and support, please contact [goran@mihelcic.net](mailto:goran@mihelcic.net).

## Project Status
This project is actively maintained and undergoing regular updates.