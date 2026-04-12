const fs = require('fs');
const path = 'c:\\Users\\mrkfk\\Desktop\\EGM-Project\\EGM.Frontend\\src\\app\\pages\\sokak-olay-ekle\\sokak-olay-ekle.ts';

let content = fs.readFileSync(path, 'utf8');

// Fix: Add latitude/longitude to auto-fill patchValue calls (3 occurrences)
const oldPatch = 'baslangicKonum: `${coords.latitude.toFixed(6)}, ${coords.longitude.toFixed(6)}`\r\n          }, { emitEvent: false });';

const newPatch = 'baslangicKonum: `${coords.latitude.toFixed(6)}, ${coords.longitude.toFixed(6)}`,\r\n            latitude: coords.latitude,\r\n            longitude: coords.longitude\r\n          }, { emitEvent: false });';

const count = content.split(oldPatch).length - 1;
console.log(`Found ${count} occurrences of patchValue pattern`);
content = content.split(oldPatch).join(newPatch);

fs.writeFileSync(path, content, 'utf8');
console.log('Fix applied successfully');
