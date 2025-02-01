const { execSync } = require('child_process');
const fs = require('fs');

try {
    const commitId = execSync('git log -1 --pretty=format:"%h - %an: %s"').toString().trim();
    const versionData = {
        gitCommitId: commitId,
    };

    // Create a JavaScript file containing a variable
    const jsContent = `
        export const versionData = ${JSON.stringify(versionData)};
      `;

    fs.writeFileSync('./src/version.js', jsContent, 'utf-8');
    console.log('Version file created successfully.');
} catch (error) {
    console.error('Error creating version file:', error);
}