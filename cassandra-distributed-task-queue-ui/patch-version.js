/* eslint-disable @typescript-eslint/no-var-requires */

// eslint-disable-next-line @typescript-eslint/no-require-imports
const cp = require("child_process");

function execAsync(command, options) {
    return new Promise((resolve, reject) =>
        cp.exec(command, options, (error, stdout, stderr) => {
            if (error) {
                reject(error);
            } else {
                resolve({ stdout: stdout, stderr: stderr });
            }
        })
    );
}

async function setPackageVersion(packageDirectory) {
    const versionText = await execAsync("dotnet nbgv get-version --variable NpmPackageVersion");
    if (versionText.stderr) {
        throw versionText.stderr;
    }

    const npmPackageVersion = versionText.stdout.trim();
    console.log(`Setting package version to ${npmPackageVersion}`);
    const result = await execAsync(`npm version ${npmPackageVersion} --no-git-tag-version --allow-same-version`, {
        cwd: packageDirectory,
    });
    if (result.stderr) {
        console.log(result.stderr);
    }
}

setPackageVersion("./dist");
