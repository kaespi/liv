#pragma once

#include <QDir>
#include <QStringList>

class FileSystemWalker {
public:
  FileSystemWalker(const QString &file);

  QString getCurrentFile() const;
  QString getNextFile();
  QString getPrevFile();

private:
  QDir m_dir{};
  QStringList m_filesInDir{};
  qsizetype m_currentFileIndex{0};
};
