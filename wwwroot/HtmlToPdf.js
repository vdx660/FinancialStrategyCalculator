export function generatePdfCompressed(htmlOrElement) {
    const doc = new jspdf.jsPDF({
        orientation: 'l',
        unit: 'pt', // points, pixels won't work properly
        format: 'A2',
        margin: { // set 40 points margin for all sides
            top: 100,
            left: 40,
            right: 40,
            bottom: 100
        }
    });

    return new Promise((resolve, reject) => {
        doc.html(htmlOrElement, {
            callback: doc => {
                const output = doc.output("arraybuffer");
                resolve(new Uint8Array(output));
            }
        });
    });
}

export function generatePdfExpanded(htmlOrElement) {
    const doc = new jspdf.jsPDF({
        orientation: 'l',
        unit: 'pt', // points, pixels won't work properly
        format: 'A0',
        margin: { // set 40 points margin for all sides
            top: 100,
            left: 40,
            right: 40,
            bottom: 100
        }
    });

    return new Promise((resolve, reject) => {
        doc.html(htmlOrElement, {
            callback: doc => {
                const output = doc.output("arraybuffer");
                resolve(new Uint8Array(output));
            }
        });
    });
}

export function generateImageToPdfCompressed(image) {
    const doc = new jspdf.jsPDF({
        orientation: 'l',
        unit: 'pt', // points, pixels won't work properly
        format: 'A2',
        margin: { // set 40 points margin for all sides
            top: 100,
            left: 40,
            right: 40,
            bottom: 100
        }
    });

    return new Promise((resolve, reject) => {
        doc.addImage(image, {
            callback: doc => {
                const output = doc.output("arraybuffer");
                resolve(new Uint8Array(output));
            }
        });
    });
}
