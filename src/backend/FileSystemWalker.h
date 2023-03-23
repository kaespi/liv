#pragma once

#include <QString>

class FileSystemWalker {
public:
  FileSystemWalker(const QString &file);

  QString getCurrentFile() const;
  QString getNextFile();
  QString getPrevFile();

private:
  QString m_file{};
};
